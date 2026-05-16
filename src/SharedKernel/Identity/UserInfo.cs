using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SharedKernel.Compliance;

namespace SharedKernel.Identity;

public sealed class UserInfo
{
    [EuiiData]
    public string UserId { get; }
    public string Name { get; }
    public string[] Roles { get; }
    public string[] Groups { get; }
    public string[] Wids { get; }

    public const string UserIdClaimType = JwtRegisteredClaimNames.Sub;
    public const string NameClaimType = JwtRegisteredClaimNames.Name;
    private const string RoleClaimType = "roles";
    private const string GroupsClaimType = "groups";
    private const string WidsClaimType = "wids";

    public UserInfo(string userId, string name, string[] roles, string[] groups, string[] wids)
    {
        UserId = userId;
        Name = name;
        Roles = roles;
        Groups = groups;
        Wids = wids;
    }

    public bool IsAnonymous => UserId == Anonymous.UserId;

    public static UserInfo FromClaimsPrincipal(ClaimsPrincipal principal) =>
        new(GetRequiredClaim(principal, UserIdClaimType),
            GetRequiredClaim(principal, NameClaimType),
            [.. principal.FindAll(RoleClaimType).Select(c => c.Value)],
            [.. principal.FindAll(GroupsClaimType).Select(c => c.Value)],
            [.. principal.FindAll(WidsClaimType).Select(c => c.Value)]);

    public ClaimsPrincipal ToClaimsPrincipal() =>
        new(new ClaimsIdentity(
            Roles.Select(role => new Claim(RoleClaimType, role))
                .Concat(Groups.Select(role => new Claim(GroupsClaimType, role)))
                .Concat(Wids.Select(role => new Claim(WidsClaimType, role)))
                .Concat([
                    new Claim(UserIdClaimType, UserId),
                    new Claim(NameClaimType, Name),
                ]),
            authenticationType: nameof(UserInfo),
            nameType: NameClaimType,
            roleType: RoleClaimType));

    private static string GetRequiredClaim(ClaimsPrincipal principal,
        string claimType) =>
            principal.FindFirst(claimType)?.Value ??
            throw new InvalidOperationException(
                $"Could not find required '{claimType}' claim.");

    public static readonly UserInfo Anonymous = new(
        "anonymous",
        "Anonymous",
        [],
        [],
        []);
}