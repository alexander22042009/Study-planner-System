namespace StudyPlanner.Core.Models.Common;

public class SearchQuery
{
    public string? SearchTerm { get; set; }

    public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);

    public string GetNormalizedSearchTerm() => SearchTerm?.Trim() ?? string.Empty;
}
