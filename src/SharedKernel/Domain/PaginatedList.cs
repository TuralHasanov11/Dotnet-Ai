using System.Collections.ObjectModel;

namespace SharedKernel.Domain;

public class PaginatedList<T> : List<T>
{
    public int Page { get; }

    public int TotalPages { get; }

    public int PageSize { get; }

    public long TotalCount { get; }

    public PaginatedList(ReadOnlyCollection<T> items, long count, int pageIndex, int pageSize)
    {
        Page = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        PageSize = pageSize;
        TotalCount = count;

        this.AddRange(items);
    }

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}

