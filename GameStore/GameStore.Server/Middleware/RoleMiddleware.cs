using Microsoft.AspNetCore.Identity;
using Games_Store.Models;

namespace Games_Store.Middleware
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    foreach (var role in roles)
                    {
                        context.Items[$"Role_{role}"] = true;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class RoleMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleMiddleware>();
        }
    }
} 