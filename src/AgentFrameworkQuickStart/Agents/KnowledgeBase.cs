namespace AgentFrameworkQuickStart.Agents;

public interface IKnowledgeBase
{
    KnowledgeDocument Add(KnowledgeDocumentDraft draft);

    IReadOnlyList<KnowledgeDocument> GetAll();

    KnowledgeDocument? GetById(Guid id);

    IReadOnlyList<KnowledgeMatch> Search(string query, int limit);

    void Clear();
}

public sealed class InMemoryKnowledgeBase : IKnowledgeBase
{
    private readonly object _gate = new();
    private readonly List<KnowledgeDocument> _documents = [];

    public KnowledgeDocument Add(KnowledgeDocumentDraft draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(draft.Content);

        var document = new KnowledgeDocument(
            Guid.NewGuid(),
            draft.Title.Trim(),
            draft.Content.Trim(),
            string.IsNullOrWhiteSpace(draft.Source) ? null : draft.Source.Trim(),
            (draft.Tags ?? [])
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            DateTimeOffset.UtcNow);

        lock (_gate)
        {
            _documents.Add(document);
        }

        return document;
    }

    public IReadOnlyList<KnowledgeDocument> GetAll()
    {
        lock (_gate)
        {
            return _documents
                .OrderByDescending(document => document.UploadedAt)
                .ToArray();
        }
    }

    public KnowledgeDocument? GetById(Guid id)
    {
        lock (_gate)
        {
            return _documents.FirstOrDefault(document => document.Id == id);
        }
    }

    public IReadOnlyList<KnowledgeMatch> Search(string query, int limit)
    {
        if (string.IsNullOrWhiteSpace(query) || limit <= 0)
        {
            return [];
        }

        var tokens = Tokenize(query).ToArray();

        if (tokens.Length == 0)
        {
            return [];
        }

        KnowledgeDocument[] snapshot;

        lock (_gate)
        {
            snapshot = _documents.ToArray();
        }

        return snapshot
            .Select(document => Score(document, tokens))
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenByDescending(match => match.Document.UploadedAt)
            .Take(limit)
            .ToArray();
    }

    public void Clear()
    {
        lock (_gate)
        {
            _documents.Clear();
        }
    }

    private static KnowledgeMatch Score(KnowledgeDocument document, IReadOnlyList<string> tokens)
    {
        var title = document.Title;
        var content = document.Content;
        var score = 0;

        foreach (var token in tokens)
        {
            if (title.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }

            score += CountOccurrences(content, token) * 4;

            if (document.Tags.Any(tag => tag.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                score += 3;
            }
        }

        return new KnowledgeMatch(document, score, CreateExcerpt(content, tokens));
    }

    private static int CountOccurrences(string source, string token)
    {
        var count = 0;
        var index = 0;

        while (index >= 0)
        {
            index = source.IndexOf(token, index, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                count++;
                index += token.Length;
            }
        }

        return count;
    }

    private static string CreateExcerpt(string content, IReadOnlyList<string> tokens)
    {
        var normalized = content.ReplaceLineEndings(" ").Trim();

        if (normalized.Length <= 220)
        {
            return normalized;
        }

        foreach (var token in tokens)
        {
            var index = normalized.IndexOf(token, StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
            {
                var start = Math.Max(0, index - 80);
                var length = Math.Min(220, normalized.Length - start);
                var excerpt = normalized.Substring(start, length);

                return start > 0 ? $"...{excerpt}" : excerpt;
            }
        }

        return normalized[..220] + "...";
    }

    private static IEnumerable<string> Tokenize(string query)
    {
        var token = new System.Text.StringBuilder();

        foreach (var character in query)
        {
            if (char.IsLetterOrDigit(character))
            {
                token.Append(char.ToLowerInvariant(character));
                continue;
            }

            if (token.Length > 0)
            {
                yield return token.ToString();
                token.Clear();
            }
        }

        if (token.Length > 0)
        {
            yield return token.ToString();
        }
    }
}
