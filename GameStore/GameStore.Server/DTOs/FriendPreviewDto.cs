using System.Collections.Generic;

namespace Games_Store.DTOs
{
    public class FriendPreviewDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }


    public class UserProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? Bio { get; set; }
        public int GamesOwned { get; set; }
        public int WishlistCount { get; set; }
        public int FriendsCount { get; set; }
        public List<FriendPreviewDto> FriendsPreview { get; set; } = new List<FriendPreviewDto>();
        public string FriendshipStatus { get; set; } = "none";
    }
} 