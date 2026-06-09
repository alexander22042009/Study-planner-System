using StudyPlanner.Core.Enums;

namespace StudyPlanner.Core.Models.Common;

public class SortQuery
{
    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    public bool IsDescending => SortDirection == SortDirection.Descending;
}
