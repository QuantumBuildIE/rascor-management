namespace Rascor.Modules.SiteAttendance.Application.DTOs;

/// <summary>
/// Request to manually process attendance summaries for a date range
/// </summary>
public record ProcessSummariesRequest
{
    /// <summary>
    /// Start date for processing. Defaults to 30 days ago if not specified.
    /// </summary>
    public DateOnly? FromDate { get; init; }

    /// <summary>
    /// End date for processing. Defaults to yesterday if not specified.
    /// </summary>
    public DateOnly? ToDate { get; init; }
}

/// <summary>
/// Result of processing attendance summaries
/// </summary>
public record ProcessSummariesResult
{
    /// <summary>
    /// Number of dates that were processed
    /// </summary>
    public int DatesProcessed { get; init; }

    /// <summary>
    /// Number of summaries created or updated
    /// </summary>
    public int SummariesCreated { get; init; }

    /// <summary>
    /// Number of events that were processed
    /// </summary>
    public int EventsProcessed { get; init; }

    /// <summary>
    /// Start date that was processed
    /// </summary>
    public DateOnly FromDate { get; init; }

    /// <summary>
    /// End date that was processed
    /// </summary>
    public DateOnly ToDate { get; init; }

    /// <summary>
    /// Details of processing per date
    /// </summary>
    public IEnumerable<DateProcessingDetail> Details { get; init; } = [];
}

/// <summary>
/// Processing details for a single date
/// </summary>
public record DateProcessingDetail
{
    public DateOnly Date { get; init; }
    public int EventsProcessed { get; init; }
    public int SummariesCreated { get; init; }
}

/// <summary>
/// Status of attendance event processing
/// </summary>
public record ProcessingStatusResult
{
    /// <summary>
    /// Total number of events in the system
    /// </summary>
    public int TotalEvents { get; init; }

    /// <summary>
    /// Number of processed events
    /// </summary>
    public int ProcessedEvents { get; init; }

    /// <summary>
    /// Number of unprocessed events
    /// </summary>
    public int UnprocessedEvents { get; init; }

    /// <summary>
    /// Total number of summaries
    /// </summary>
    public int TotalSummaries { get; init; }

    /// <summary>
    /// Summary counts by date
    /// </summary>
    public IEnumerable<DateSummaryCount> SummariesByDate { get; init; } = [];

    /// <summary>
    /// Event counts by date
    /// </summary>
    public IEnumerable<DateEventCount> EventsByDate { get; init; } = [];
}

/// <summary>
/// Summary count for a date
/// </summary>
public record DateSummaryCount
{
    public DateOnly Date { get; init; }
    public int Count { get; init; }
}

/// <summary>
/// Event count for a date
/// </summary>
public record DateEventCount
{
    public DateOnly Date { get; init; }
    public int TotalEvents { get; init; }
    public int ProcessedEvents { get; init; }
    public int UnprocessedEvents { get; init; }
}
