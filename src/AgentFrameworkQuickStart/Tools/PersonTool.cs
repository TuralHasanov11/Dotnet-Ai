using System.ComponentModel;
using System.Text.Json;
using SharedKernel.Identity;

namespace AgentFrameworkQuickStart.Tools;

public static class PersonTool
{
    [Description("Get information about a person.")]
    public static string GetPersonInfo([Description("The name of the person to get information about.")] string name)
    {
        var user = new UserInfo(
            userId: Guid.NewGuid().ToString(),
            name: name,
            roles: ["Admin"],
            groups: ["Group1", "Group2"],
            wids: ["Wid1", "Wid2"]
        );

        return $"Here is some information about {name}: {JsonSerializer.Serialize(user)}";
    }
}