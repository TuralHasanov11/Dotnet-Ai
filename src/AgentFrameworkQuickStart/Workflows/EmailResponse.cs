using System.Text.Json.Serialization;

namespace AgentFrameworkQuickStart.Workflows;

public sealed class EmailResponse
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}