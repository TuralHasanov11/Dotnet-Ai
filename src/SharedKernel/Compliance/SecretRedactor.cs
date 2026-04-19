using Microsoft.Extensions.Compliance.Redaction;

namespace SharedKernel.Compliance;

/// <summary>
/// Redacts sensitive information from text.
/// Redacts any occurrence of the secret value with stars placeholder.
/// </summary>
public class SecretRedactor : Redactor
{
    private const string SecretValue = "*****";

    public override int GetRedactedLength(ReadOnlySpan<char> input)
    {
        return SecretValue.Length;
    }

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        SecretValue.CopyTo(destination);
        return SecretValue.Length;
    }
}