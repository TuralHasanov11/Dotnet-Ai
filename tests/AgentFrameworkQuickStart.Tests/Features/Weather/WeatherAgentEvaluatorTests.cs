using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkQuickStart.Tests.Features.Weather
{
    public class WeatherAgentEvaluatorTests(BaseFactory factory) : BaseIntegrationTest(factory)
    {
        public const string WeatherAgentKey = "WeatherAgent";

        [Fact]
        public async Task ResponseEvaluation()
        {
            var agent = factory.Services.GetRequiredKeyedService<AIAgent>(WeatherAgentKey);

            var local = new LocalEvaluator(
                EvalChecks.KeywordCheck("weather", "temperature"),  // Response must contain these keywords
                EvalChecks.ToolCalledCheck("get_weather")            // Agent must have called this tool
            );

            var results = await agent.EvaluateAsync(
                [
                    "What's the weather in Seattle?",
                ],
                local,
                cancellationToken: TestContext.Current.CancellationToken);

            results.AssertAllPassed(); 
        }
    }
}