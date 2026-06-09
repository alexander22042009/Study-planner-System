namespace StudyPlanner.Core.Models.Common;

public class PaginationQuery
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public int PageNumber { get; set; } = DefaultPageNumber;

    public int PageSize { get; set; } = DefaultPageSize;

    public void Normalize()
    {
        if (PageNumber < 1)
        {
            PageNumber = DefaultPageNumber;
        }

        if (PageSize < 1)
        {
            PageSize = DefaultPageSize;
        }
        else if (PageSize > MaxPageSize)
        {
            PageSize = MaxPageSize;
        }
    }
}
