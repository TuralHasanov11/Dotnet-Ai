using SharedKernel.Domain;

namespace SharedKernel.Tests;

public class PaginatedListTests
{
    [Fact]
    public void HasNextPageAndHasPreviousPage_ShouldReflectCurrentPage()
    {
        var items = new List<int> { 1, 2, 3 }.AsReadOnly();

        var paginatedList = new PaginatedList<int>(items, count: 9, pageIndex: 2, pageSize: 3);

        Assert.True(paginatedList.HasPreviousPage);
        Assert.True(paginatedList.HasNextPage);
        Assert.Equal(3, paginatedList.TotalPages);
    }
}