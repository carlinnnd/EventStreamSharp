namespace EventStreamSharp.Dashboard.Models;

public class ServiceMetric
{
    public int UploadId { get; set; }
    public string ServiceName { get; set; } = "";
    public int TotalRequests { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public double SuccessRate { get; set; }
    public double ErrorRate { get; set; }
    public double AverageDurationMs { get; set; }
    public int MinDuration { get; set; }
    public int MaxDuration { get; set; }
}
