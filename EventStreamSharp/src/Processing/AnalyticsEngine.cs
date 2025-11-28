using System.Collections.Generic;
using EventStreamSharp.Domain;
using System.Linq;

namespace EventStreamSharp.Processing;

public class AnalyticsEngine
{
    public List <ServiceStats> CalculateStats (List <EventRecord> eventos)
    {
        var stats = new List<ServiceStats>();
        var grupos = eventos.GroupBy(e => e.ServiceName);
        foreach (var grupo in grupos)
        {
            var serviceName = grupo.Key;
            var totalRequests = grupo.Count();
            var successCount = grupo.Count(e => e.Success);
            var errorCount = totalRequests - successCount;
            var successRate = totalRequests > 0 ? (double)successCount / totalRequests * 100 : 0;
            var errorRate = totalRequests > 0 ? (double)errorCount / totalRequests * 100 : 0;
            var averageDurationMs = grupo.Any() ? grupo.Average(e => e.DurationMs) : 0;
            var maxDuration = grupo.Any() ? grupo.Max(e => e.DurationMs) : 0;
            var minDuration = grupo.Any() ? grupo.Min(e => e.DurationMs) : 0;

            var serviceStats = new ServiceStats(
                serviceName,
                totalRequests,
                successCount,
                errorCount,
                successRate,
                errorRate,
                averageDurationMs,
                maxDuration,
                minDuration
            );

            stats.Add(serviceStats);
        }
        return stats;
    }
    
}