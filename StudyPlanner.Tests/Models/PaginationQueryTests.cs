using StudyPlanner.Core.Models.Common;

namespace StudyPlanner.Tests.Models;

[TestFixture]
public class PaginationQueryTests
{
    [Test]
    public void Normalize_ClampsInvalidValues()
    {
        var query = new PaginationQuery { PageNumber = 0, PageSize = 500 };
        query.Normalize();

        Assert.That(query.PageNumber, Is.EqualTo(PaginationQuery.DefaultPageNumber));
        Assert.That(query.PageSize, Is.EqualTo(PaginationQuery.MaxPageSize));
    }

    [Test]
    public void PagedResult_Create_CalculatesTotalPages()
    {
        var result = PagedResult<string>.Create(["a", "b"], 1, 10, 25);

        Assert.That(result.TotalPages, Is.EqualTo(3));
        Assert.That(result.HasNextPage, Is.True);
    }
}
