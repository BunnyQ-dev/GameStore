using Games_Store.Models;

namespace Games_Store.Services
{
    public interface IUserEventHandler
    {
        Task HandleUserCreatedAsync(ApplicationUser user);
    }
} 