using Microsoft.EntityFrameworkCore;
using EventStreamSharp.API.Data;

namespace EventStreamSharp.API.Data;

public class ServiceMetricEntity
{
    public int Id { get; set; }
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

    public UploadEntity Upload { get; set; } = null!;
}
public class MetricsContext : DbContext
{
    public MetricsContext(DbContextOptions<MetricsContext> options)
        : base(options)
    {
    }

    public DbSet<UploadEntity> Uploads { get; set; }
    public DbSet<ServiceMetricEntity> Metrics { get; set; }
}
