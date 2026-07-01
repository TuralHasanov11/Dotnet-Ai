using System.ComponentModel;
using System.Text.Json;
using Microsoft.Agents.AI;

namespace AgentFrameworkQuickStart.Skills;

#pragma warning disable MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
internal sealed class UnitConverterSkill : AgentClassSkill<UnitConverterSkill>
#pragma warning restore MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
{
#pragma warning disable MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
#pragma warning restore MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        "unit-converter",
        "Convert between common units using a multiplication factor. Use when asked to convert miles, kilometers, pounds, or kilograms.");

    protected override string Instructions => """
        Use this skill when the user asks to convert between units.

        1. Review the conversion-table resource to find the correct factor.
        2. Use the convert script, passing the value and factor from the table.
        3. Present the result clearly with both units.
        """;

#pragma warning disable MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    [AgentSkillResource("conversion-table")]
#pragma warning restore MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    [Description("Lookup table of multiplication factors for common unit conversions.")]
    public string ConversionTable => """
        # Conversion Tables
        Formula: **result = value × factor**
        | From       | To         | Factor   |
        |------------|------------|----------|
        | miles      | kilometers | 1.60934  |
        | kilometers | miles      | 0.621371 |
        | pounds     | kilograms  | 0.453592 |
        | kilograms  | pounds     | 2.20462  |
        """;

#pragma warning disable MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    [AgentSkillScript("convert")]
#pragma warning restore MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    [Description("Multiplies a value by a conversion factor and returns the result as JSON.")]
    private static string ConvertUnits(double value, double factor)
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    }
}