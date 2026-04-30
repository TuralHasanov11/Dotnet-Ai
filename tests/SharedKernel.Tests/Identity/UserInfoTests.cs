using System.Security.Claims;
using SharedKernel.Identity;

namespace SharedKernel.Tests.Identity;

public sealed class UserInfoTests
{
    private static ClaimsPrincipal CreateClaimsPrincipal(
        string userId = "user-123",
        string name = "John Doe",
        string[] roles = null!,
        string[] groups = null!,
        string[] wids = null!)
    {
        roles ??= ["admin", "user"];
        groups ??= ["group1", "group2"];
        wids ??= ["wid1", "wid2"];

        var claims = new List<Claim>
        {
            new(UserInfo.UserIdClaimType, userId),
            new(UserInfo.NameClaimType, name),
        };

        claims.AddRange(roles.Select(role => new Claim("roles", role)));
        claims.AddRange(groups.Select(group => new Claim("groups", group)));
        claims.AddRange(wids.Select(wid => new Claim("wids", wid)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public void FromClaimsPrincipal_WithValidClaims_ReturnsUserInfo()
    {
        // Arrange
        var principal = CreateClaimsPrincipal(
            "user-123",
            "John Doe",
            ["admin", "user"],
            ["group1", "group2"],
            ["wid1"]);

        // Act
        var userInfo = UserInfo.FromClaimsPrincipal(principal);

        // Assert
        Assert.Equal("user-123", userInfo.UserId);
        Assert.Equal("John Doe", userInfo.Name);
        Assert.Equal(["admin", "user"], userInfo.Roles);
        Assert.Equal(["group1", "group2"], userInfo.Groups);
        Assert.Equal(["wid1"], userInfo.Wids);
    }

    [Fact]
    public void FromClaimsPrincipal_WithEmptyRoles_ReturnsEmptyRolesArray()
    {
        // Arrange
        var principal = CreateClaimsPrincipal(
            roles: [],
            groups: ["group1"],
            wids: ["wid1"]);

        // Act
        var userInfo = UserInfo.FromClaimsPrincipal(principal);

        // Assert
        Assert.Empty(userInfo.Roles);
        Assert.Equal(["group1"], userInfo.Groups);
    }

    [Fact]
    public void FromClaimsPrincipal_WithEmptyGroups_ReturnsEmptyGroupsArray()
    {
        // Arrange
        var principal = CreateClaimsPrincipal(
            roles: ["admin"],
            groups: [],
            wids: ["wid1"]);

        // Act
        var userInfo = UserInfo.FromClaimsPrincipal(principal);

        // Assert
        Assert.Equal(["admin"], userInfo.Roles);
        Assert.Empty(userInfo.Groups);
    }

    [Fact]
    public void FromClaimsPrincipal_WithEmptyWids_ReturnsEmptyWidsArray()
    {
        // Arrange
        var principal = CreateClaimsPrincipal(
            roles: ["admin"],
            groups: ["group1"],
            wids: []);

        // Act
        var userInfo = UserInfo.FromClaimsPrincipal(principal);

        // Assert
        Assert.Equal(["admin"], userInfo.Roles);
        Assert.Equal(["group1"], userInfo.Groups);
        Assert.Empty(userInfo.Wids);
    }

    [Fact]
    public void FromClaimsPrincipal_MissingUserIdClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(UserInfo.NameClaimType, "John Doe"),
            new Claim("roles", "admin"),
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => UserInfo.FromClaimsPrincipal(principal));
        Assert.Contains("Could not find required", exception.Message);
        Assert.Contains(UserInfo.UserIdClaimType, exception.Message);
    }

    [Fact]
    public void FromClaimsPrincipal_MissingNameClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(UserInfo.UserIdClaimType, "user-123"),
            new Claim("roles", "admin"),
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => UserInfo.FromClaimsPrincipal(principal));
        Assert.Contains("Could not find required", exception.Message);
        Assert.Contains(UserInfo.NameClaimType, exception.Message);
    }

    [Fact]
    public void FromClaimsPrincipal_WithMultipleRoles_ReturnsAllRoles()
    {
        // Arrange
        var roles = new string[] { "admin", "user", "editor" };
        var principal = CreateClaimsPrincipal(roles: roles);

        // Act
        var userInfo = UserInfo.FromClaimsPrincipal(principal);

        // Assert
        Assert.Equal(roles, userInfo.Roles);
    }

    [Fact]
    public void ToClaimsPrincipal_WithUserInfo_ReturnsClaimsPrincipalWithAllClaims()
    {
        // Arrange
        var userInfo = new UserInfo(
            userId: "user-123",
            name: "John Doe",
            roles: ["admin", "user"],
            groups: ["group1", "group2"],
            wids: ["wid1"]
        );

        // Act
        var principal = userInfo.ToClaimsPrincipal();

        // Assert
        var identity = principal.Identity as ClaimsIdentity;
        Assert.NotNull(identity);
        Assert.Equal(nameof(UserInfo), identity.AuthenticationType);
        Assert.Equal(UserInfo.NameClaimType, identity.NameClaimType);
        Assert.Equal("roles", identity.RoleClaimType);

        Assert.Equal("user-123", principal.FindFirst(UserInfo.UserIdClaimType)?.Value);
        Assert.Equal("John Doe", principal.FindFirst(UserInfo.NameClaimType)?.Value);
        Assert.Equal(new string[] { "admin", "user" }, 
            principal.FindAll("roles").Select(c => c.Value).ToList());
        Assert.Equal(new string[] { "group1", "group2" }, 
            principal.FindAll("groups").Select(c => c.Value).ToList());
        Assert.Equal(new string[] { "wid1" }, 
            principal.FindAll("wids").Select(c => c.Value).ToList());
    }

    [Fact]
    public void ToClaimsPrincipal_WithEmptyCollections_ReturnsClaimsPrincipalWithOnlyUserClaims()
    {
        // Arrange
        var userInfo = new UserInfo(
            userId: "user-123",
            name: "John Doe",
            roles: [],
            groups: [],
            wids: []
        );

        // Act
        var principal = userInfo.ToClaimsPrincipal();

        // Assert
        Assert.Equal("user-123", principal.FindFirst(UserInfo.UserIdClaimType)?.Value);
        Assert.Equal("John Doe", principal.FindFirst(UserInfo.NameClaimType)?.Value);
        Assert.Empty(principal.FindAll("roles"));
        Assert.Empty(principal.FindAll("groups"));
        Assert.Empty(principal.FindAll("wids"));
    }

    [Fact]
    public void RoundTrip_FromClaimsPrincipalToClaimsPrincipal_PreservesAllData()
    {
        // Arrange
        var originalPrincipal = CreateClaimsPrincipal(
            "user-456",
            "Jane Smith",
            ["superadmin", "user"],
            ["group1", "group2", "group3"],
            ["wid1", "wid2"]);

        // Act
        var userInfo = UserInfo.FromClaimsPrincipal(originalPrincipal);
        var reconstructedPrincipal = userInfo.ToClaimsPrincipal();

        // Assert
        Assert.Equal("user-456", reconstructedPrincipal.FindFirst(UserInfo.UserIdClaimType)?.Value);
        Assert.Equal("Jane Smith", reconstructedPrincipal.FindFirst(UserInfo.NameClaimType)?.Value);
        Assert.Equal(
            userInfo.Roles,
            reconstructedPrincipal.FindAll("roles").Select(c => c.Value).ToList());
        Assert.Equal(
            userInfo.Groups,
            reconstructedPrincipal.FindAll("groups").Select(c => c.Value).ToList());
        Assert.Equal(
            userInfo.Wids,
            reconstructedPrincipal.FindAll("wids").Select(c => c.Value).ToList());
    }

    [Fact]
    public void UserInfo_IsSealed()
    {
        // Assert
        Assert.True(typeof(UserInfo).IsSealed, "UserInfo should be sealed");
    }

    [Fact]
    public void UserInfo_PropertiesAreInitOnly()
    {
        // Arrange

        // Act & Assert - Attempting to reassign init-only properties should fail
        var properties = typeof(UserInfo).GetProperties();
        foreach (var property in properties)
        {
            var setMethod = property.GetSetMethod();
            Assert.Null(setMethod);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("user-with-spaces ")]
    public void FromClaimsPrincipal_WithVariousUserIds_ReturnsCorrectValue(string userId)
    {
        // Arrange
        var claims = new[]
        {
            new Claim(UserInfo.UserIdClaimType, userId),
            new Claim(UserInfo.NameClaimType, "John Doe"),
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        // Act
        var userInfo = UserInfo.FromClaimsPrincipal(principal);

        // Assert
        Assert.Equal(userId, userInfo.UserId);
    }

    [Fact]
    public void FromClaimsPrincipal_WithDuplicateRoles_IncludesAllDuplicates()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(UserInfo.UserIdClaimType, "user-123"),
            new Claim(UserInfo.NameClaimType, "John Doe"),
            new Claim("roles", "admin"),
            new Claim("roles", "admin"),
            new Claim("roles", "user"),
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        // Act
        var userInfo = UserInfo.FromClaimsPrincipal(principal);

        // Assert
        Assert.Equal(["admin", "admin", "user"], userInfo.Roles);
    }
}