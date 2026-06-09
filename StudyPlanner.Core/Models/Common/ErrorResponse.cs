namespace StudyPlanner.Core.Models.Common;

public class ErrorResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? TraceId { get; set; }

    public IEnumerable<string>? Errors { get; set; }

    public static ErrorResponse Create(int statusCode, string message, string? traceId = null, IEnumerable<string>? errors = null) =>
        new()
        {
            StatusCode = statusCode,
            Message = message,
            TraceId = traceId,
            Errors = errors
        };
}
