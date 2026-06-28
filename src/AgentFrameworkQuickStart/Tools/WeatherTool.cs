using System.ComponentModel;

namespace AgentFrameworkQuickStart.Tools;

public static class WeatherTool
{
    [Description("Get the weather for a given location.")]
    public static string GetWeather([Description("The location to get the weather for.")] string location)
        => $"The weather in {location} is cloudy with a high of 15°C temperature.";
}