
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace EmployeeManagement.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimRequirement>
    {
        /*protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimRequirement requirement)
        {
            var authFilterContext = context.Resource as AuthorizationFilterContext;

            if (authFilterContext == null)
                return Task.CompletedTask;

            string? logUserId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            string? userBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];

            if(context.User.IsInRole("Admin") && context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "Edit Role") && userBeingEdited.ToLower() != logUserId.ToLower())
                context.Succeed(requirement);
            
            return Task.CompletedTask;
        }*/

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CanEditOnlyOtherAdminRolesAndClaimsHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            string? loggedInUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? userBeingEdited = httpContext?.Request.Query["userId"];

            if (string.IsNullOrEmpty(userBeingEdited))
            {
                userBeingEdited = httpContext?.GetRouteValue("userId")?.ToString();
            }

            if (!string.IsNullOrEmpty(loggedInUserId) &&
                !string.IsNullOrEmpty(userBeingEdited) &&
                context.User.IsInRole("Admin")  &&
                context.User.HasClaim(c => c.Type == "Edit Role" && c.Value == "Edit Role") &&
                !string.Equals(userBeingEdited, loggedInUserId, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
