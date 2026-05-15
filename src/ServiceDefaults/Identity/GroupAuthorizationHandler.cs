using Microsoft.AspNetCore.Authorization;

namespace ServiceDefaults.Identity;

public sealed class GroupAuthorizationHandler : AuthorizationHandler<GroupRequirementAttribute>
{
    public const string ClaimType = "groups";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GroupRequirementAttribute requirement)
    {
        if (requirement.Groups.Any(group => context.User.HasClaim(c => c.Type == ClaimType && c.Value == group)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}