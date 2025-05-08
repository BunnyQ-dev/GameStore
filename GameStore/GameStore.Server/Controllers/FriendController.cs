using Games_Store.Data;
using Games_Store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace Games_Store.Controllers
{
    [Route("api/friends")]
    [ApiController]
    [Authorize]
    public class FriendController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FriendController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/friends
        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friends = await _context.Friends
                .Where(f => f.UserId == userId || f.FriendId == userId)
                .Include(f => f.User).ThenInclude(u => u.PlaySessions).ThenInclude(ps => ps.Game)
                .Include(f => f.FriendUser).ThenInclude(u => u.PlaySessions).ThenInclude(ps => ps.Game)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var friendDtos = friends.Select(f => {
                var isCurrentUserInitiator = f.UserId == userId;
                var friendUser = isCurrentUserInitiator ? f.FriendUser : f.User;
                var activeSession = friendUser.PlaySessions?.OrderByDescending(ps => ps.StartTime).FirstOrDefault(ps => ps.EndTime == null);
                var isOnline = (friendUser.LastLoginDate.HasValue && (now - friendUser.LastLoginDate.Value).TotalMinutes < 10)
                    || (activeSession != null);
                var status = activeSession != null ? $"In game: {activeSession.Game?.Title ?? "Unknown"}" : (isOnline ? "Online" : "Offline");
                return new FriendDto
                {
                    UserId = isCurrentUserInitiator ? f.UserId : f.FriendId,
                    UserName = isCurrentUserInitiator ? f.User.UserName : f.FriendUser.UserName,
                    FriendId = isCurrentUserInitiator ? f.FriendId : f.UserId,
                    FriendName = friendUser.UserName,
                    FriendDisplayName = friendUser.DisplayName,
                    FriendAvatar = friendUser.ProfilePictureUrl,
                    FriendStatus = status,
                    FriendIsOnline = isOnline,
                    FriendshipDate = f.FriendshipDate
                };
            });
            return Ok(friendDtos);
        }

        // GET: api/friends/requests/incoming
        [HttpGet("requests/incoming")]
        public async Task<IActionResult> GetIncomingRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _context.FriendRequests
                .Where(r => r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending)
                .Include(r => r.Sender)
                .ToListAsync();

            var dtos = requests.Select(r => new FriendRequestDto
            {
                Id = r.Id,
                SenderId = r.SenderId,
                SenderName = r.Sender.UserName,
                SenderAvatar = r.Sender.ProfilePictureUrl,
                RequestDate = r.RequestDate,
                Status = r.Status.ToString()
            });
            return Ok(dtos);
        }

        // GET: api/friends/requests/outgoing
        [HttpGet("requests/outgoing")]
        public async Task<IActionResult> GetOutgoingRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _context.FriendRequests
                .Where(r => r.SenderId == userId && r.Status == FriendRequestStatus.Pending)
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .ToListAsync();

            var dtos = requests.Select(r => new FriendRequestDto
            {
                Id = r.Id,
                SenderId = r.SenderId,
                SenderName = r.Sender.UserName,
                SenderAvatar = r.Sender.ProfilePictureUrl,
                ReceiverId = r.ReceiverId,
                ReceiverName = r.Receiver.UserName,
                ReceiverAvatar = r.Receiver.ProfilePictureUrl,
                RequestDate = r.RequestDate,
                Status = r.Status.ToString()
            });
            return Ok(dtos);
        }

        // POST: api/friends/request/{receiverId}
        [HttpPost("request/{receiverId}")]
        public async Task<IActionResult> SendFriendRequest(string receiverId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == receiverId) return BadRequest("Cannot add yourself as a friend.");

            // Check if already friends
            bool alreadyFriends = await _context.Friends.AnyAsync(f =>
                (f.UserId == userId && f.FriendId == receiverId) ||
                (f.UserId == receiverId && f.FriendId == userId));
            if (alreadyFriends) return Conflict("Already friends.");

            // Check if an incoming request already exists from receiver to sender
            var existingIncomingRequest = await _context.FriendRequests.FirstOrDefaultAsync(r =>
                r.SenderId == receiverId && r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending);

            if (existingIncomingRequest != null)
            {
                // Automatically accept the existing request and create friendship
                var friend = new Friend
                {
                    UserId = existingIncomingRequest.SenderId, // receiverId
                    FriendId = existingIncomingRequest.ReceiverId, // userId
                    FriendshipDate = DateTime.UtcNow
                };
                _context.Friends.Add(friend);
                existingIncomingRequest.Status = FriendRequestStatus.Accepted;
                existingIncomingRequest.ResponseDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok("Friend request accepted and friendship created.");
            }

            // Check if an outgoing request already sent from sender to receiver
            bool alreadyOutgoingRequested = await _context.FriendRequests.AnyAsync(r =>
                r.SenderId == userId && r.ReceiverId == receiverId && r.Status == FriendRequestStatus.Pending);
            if (alreadyOutgoingRequested) return Conflict("Request already sent.");

            // Create request
            var request = new FriendRequest
            {
                SenderId = userId,
                ReceiverId = receiverId,
                RequestDate = DateTime.UtcNow,
                Status = FriendRequestStatus.Pending
            };
            _context.FriendRequests.Add(request);
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601)) // 2627=PK violation, 2601=Unique Index violation
            {
                return Conflict("Friend request already exists or could not be processed due to a conflict.");
            }
            
            return Ok("Friend request sent.");
        }

        // POST: api/friends/request/{requestId}/accept
        [HttpPost("request/{requestId}/accept")]
        public async Task<IActionResult> AcceptFriendRequest(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == userId);
            if (request == null || request.Status != FriendRequestStatus.Pending) return NotFound("Request not found or already handled.");

            // Add to friends
            var friend = new Friend
            {
                UserId = request.SenderId,
                FriendId = request.ReceiverId,
                FriendshipDate = DateTime.UtcNow
            };
            _context.Friends.Add(friend);
            request.Status = FriendRequestStatus.Accepted;
            request.ResponseDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok("Friend request accepted.");
        }

        // POST: api/friends/request/{requestId}/decline
        [HttpPost("request/{requestId}/decline")]
        public async Task<IActionResult> DeclineFriendRequest(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == userId);
            if (request == null || request.Status != FriendRequestStatus.Pending) return NotFound("Request not found or already handled.");
            request.Status = FriendRequestStatus.Declined;
            request.ResponseDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok("Friend request declined.");
        }

        // POST: api/friends/request/{requestId}/cancel
        [HttpPost("request/{requestId}/cancel")]
        public async Task<IActionResult> CancelFriendRequest(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.SenderId == userId && r.Status == FriendRequestStatus.Pending);
            if (request == null) return NotFound("Request not found or already handled.");
            request.Status = FriendRequestStatus.Canceled;
            request.ResponseDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok("Friend request canceled.");
        }

        // DELETE: api/friends/{friendId}
        [HttpDelete("{friendId}")]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friendship = await _context.Friends.FirstOrDefaultAsync(f =>
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.UserId == friendId && f.FriendId == userId));
                
            if (friendship == null) return NotFound("Friendship not found.");

            var user1Id = friendship.UserId;
            var user2Id = friendship.FriendId;

            _context.Friends.Remove(friendship);

            var relatedRequests = await _context.FriendRequests.Where(r =>
                (r.SenderId == user1Id && r.ReceiverId == user2Id) ||
                (r.SenderId == user2Id && r.ReceiverId == user1Id))
                .ToListAsync();

            if (relatedRequests.Any())
            {
                _context.FriendRequests.RemoveRange(relatedRequests);
            }

            await _context.SaveChangesAsync();
            
            return Ok("Friend removed.");
        }
    }

    public class FriendDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FriendId { get; set; }
        public string FriendName { get; set; }
        public string FriendDisplayName { get; set; }
        public string FriendAvatar { get; set; }
        public string FriendStatus { get; set; }
        public bool FriendIsOnline { get; set; }
        public DateTime FriendshipDate { get; set; }
    }

    public class FriendRequestDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string? SenderAvatar { get; set; }
        public string ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public string? ReceiverAvatar { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
    }
} 