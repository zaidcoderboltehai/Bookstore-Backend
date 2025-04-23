using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.API.Authorization
{
    /// <summary>
    /// Custom handler to process authorization results and return custom error messages for forbidden requests.
    /// </summary>
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        // Use the default handler for fallback
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            // Check if the request is forbidden and the user is authenticated
            if (authorizeResult.Forbidden && context.User.Identity.IsAuthenticated)
            {
                // Extract the required roles from the authorization policy
                var requiredRoles = policy.Requirements
                    .OfType<RolesAuthorizationRequirement>()
                    .SelectMany(r => r.AllowedRoles)
                    .Distinct()
                    .ToList();

                if (requiredRoles.Any())
                {
                    // Return a custom 403 Forbidden response with a JSON payload
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

            // Fallback to the default handler for all other scenarios
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
