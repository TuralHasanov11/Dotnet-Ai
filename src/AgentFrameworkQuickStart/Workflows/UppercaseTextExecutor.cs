using Microsoft.Agents.AI.Workflows;

namespace AgentFrameworkQuickStart.Workflows;

public static partial class UppercaseTextExecutor
{
    private static readonly Func<string, string> ToUppercaseFunc = s => s.ToUpperInvariant();

    public static ExecutorBinding GetExecutor => ToUppercaseFunc.BindAsExecutor("UppercaseTextExecutor");
}

