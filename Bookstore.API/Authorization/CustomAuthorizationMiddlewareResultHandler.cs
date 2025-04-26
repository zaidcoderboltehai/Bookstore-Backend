using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.API.Authorization
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            // ✅ Pehle non-forbidden requests ko default handler ko dedo
            if (!authorizeResult.Forbidden)
            {
                await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
                return;
            }

            // Custom handling sirf forbidden requests ke liye
            if (context.User.Identity.IsAuthenticated)
            {
                var requiredRoles = policy.Requirements
                    .OfType<RolesAuthorizationRequirement>()
                    .SelectMany(r => r.AllowedRoles)
                    .Distinct()
                    .ToList();

                if (requiredRoles.Any())
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Status = "AuthorizationFailed",
                        ErrorCode = "AUTH-403",
                        Message = $"Access denied. Requires role(s): {string.Join(", ", requiredRoles)}"
                    });
                    return;
                }
            }

            // Fallback for other forbidden scenarios
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}