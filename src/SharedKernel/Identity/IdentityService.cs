using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SharedKernel.Identity;

public class IdentityService(IHttpContextAccessor context) : IIdentityService
{
    public UserInfo User => context.HttpContext?.User is ClaimsPrincipal principal
        ? UserInfo.FromClaimsPrincipal(principal)
        : UserInfo.Anonymous;
}