using System.ComponentModel.DataAnnotations;

namespace ServiceDefaults.Identity;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required]
    public string Authority { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    public string RedirectUri { get; set; } = string.Empty;

    public IEnumerable<string> Scopes { get; set; } = [];
}