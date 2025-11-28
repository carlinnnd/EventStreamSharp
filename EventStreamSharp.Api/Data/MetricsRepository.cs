using Microsoft.EntityFrameworkCore;
using EventStreamSharp.API.Data;

namespace EventStreamSharp.API.Data;

public class MetricsRepository
{
    private readonly MetricsContext _context;

    public MetricsRepository(MetricsContext context)
    {
        _context = context;
    }

    // ----------------------
    // Salvar Upload
    // ----------------------
    public async Task<int> SaveUploadAsync(UploadEntity upload)
    {
        _context.Uploads.Add(upload);
        await _context.SaveChangesAsync();
        return upload.Id;
    }

    // ----------------------
    // Salvar Métrica por Serviço
    // ----------------------
    public async Task SaveMetricAsync(ServiceMetricEntity metric)
    {
        _context.Metrics.Add(metric);
        await _context.SaveChangesAsync();
    }

    // ----------------------
    // Buscar histórico de uploads (com navegação)
    // ----------------------
    public async Task<List<UploadEntity>> GetUploadsAsync()
    {
        return await _context
            .Uploads
            .Include(u => u.Metrics)
            .OrderByDescending(u => u.Timestamp)
            .ToListAsync();
    }

    // ----------------------
    // Histórico de um serviço específico
    // ----------------------
    public async Task<List<ServiceMetricEntity>> GetMetricsByServiceAsync(string serviceName)
    {
        return await _context
            .Metrics
            .Where(m => m.ServiceName == serviceName)
            .OrderByDescending(m => m.UploadId)
            .ToListAsync();
    }

    // ----------------------
    // Buscar métricas de um upload específico
    // ----------------------
    public async Task<List<ServiceMetricEntity>> GetMetricsByUploadAsync(int uploadId)
    {
        return await _context
            .Metrics
            .Where(m => m.UploadId == uploadId)
            .ToListAsync();
    }
}
