using Games_Store.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Games_Store.Services
{
    public class RoleAssignmentService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RoleAssignmentService> _logger;

        public RoleAssignmentService(
            IServiceProvider serviceProvider,
            ILogger<RoleAssignmentService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }
    }

    public class UserEventHandler : IUserEventHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserEventHandler> _logger;

        public UserEventHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<UserEventHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task HandleUserCreatedAsync(ApplicationUser user)
        {
            try
            {
                if (!await _userManager.IsInRoleAsync(user, "Player"))
                {
                    await _userManager.AddToRoleAsync(user, "Player");
                    _logger.LogInformation($"Added Player role to user {user.UserName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding Player role to user {user.UserName}");
            }
        }
    }
} 