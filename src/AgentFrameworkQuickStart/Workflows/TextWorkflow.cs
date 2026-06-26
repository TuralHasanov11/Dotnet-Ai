using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;

namespace AgentFrameworkQuickStart.Workflows;

public static class TextWorkflow
{
    public static readonly Func<string, string> ToUppercaseFunc = s => s.ToUpperInvariant();

    public static ExecutorBinding UppercaseTextExecutor => ToUppercaseFunc.BindAsExecutor("UppercaseTextExecutor");

    public class ReverseTextExecutor() : Executor<string, string>("ReverseTextExecutor")
    {
        public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(string.Concat(message.Reverse()));
        }
    }
}

