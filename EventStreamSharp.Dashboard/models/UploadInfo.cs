using System;
using System.Collections.Generic;

namespace EventStreamSharp.Dashboard.Models;

public class UploadInfo
{
    public int Id { get; set; }
    public string FilePath { get; set; } = "";
    public DateTime Timestamp { get; set; }

    public List<ServiceMetric> Metrics { get; set; } = new();
}
