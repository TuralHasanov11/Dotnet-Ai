using Microsoft.Agents.AI;

namespace AgentFrameworkQuickStart.Adapters;

public static class SearchAdapter
{
    public static Task<IEnumerable<TextSearchProvider.TextSearchResult>> Adapter(string query, CancellationToken cancellationToken)
    {
        // The mock search inspects the user's question and returns pre-defined snippets
        // that resemble documents stored in an external knowledge source.
        List<TextSearchProvider.TextSearchResult> results = new();

        if (query.Contains("return", StringComparison.OrdinalIgnoreCase) || query.Contains("refund", StringComparison.OrdinalIgnoreCase))
        {
            results.Add(new()
            {
                SourceName = "Contoso Outdoors Return Policy",
                SourceLink = "https://contoso.com/policies/returns",
                Text = "Customers may return any item within 30 days of delivery. Items should be unused and include original packaging. Refunds are issued to the original payment method within 5 business days of inspection."
            });
        }

        return Task.FromResult<IEnumerable<TextSearchProvider.TextSearchResult>>(results);
    }
}

