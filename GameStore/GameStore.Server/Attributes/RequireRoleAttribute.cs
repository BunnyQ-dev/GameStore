using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Games_Store.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public RequireRoleAttribute(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Перевірка, чи користувач аутентифікований
            if (context.HttpContext.User.Identity == null || !context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Перевірка, чи користувач має необхідну роль
            if (!context.HttpContext.User.IsInRole(_role))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
} 