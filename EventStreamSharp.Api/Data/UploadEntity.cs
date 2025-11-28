namespace EventStreamSharp.API.Data;

public class UploadEntity
{
    public int Id { get; set; }
    public string FilePath { get; set; } = "";
    public DateTime Timestamp { get; set; }

    public List<ServiceMetricEntity> Metrics { get; set; } = new();
}
