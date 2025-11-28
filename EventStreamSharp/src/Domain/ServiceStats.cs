namespace EventStreamSharp.Domain;

public record ServiceStats(
    string ServiceName,
    int TotalRequests,
    int SuccessCount,
    int ErrorCount,
    double SuccessRate,
    double ErrorRate,
    double AverageDurationMs,
    int MaxDuration,
    int MinDuration
);