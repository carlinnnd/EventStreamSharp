using System.Security.Cryptography.X509Certificates;
using EventStreamSharp.API;
using EventStreamSharp.API.Data;
using EventStreamSharp.Ingest;
using EventStreamSharp.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_000_000_000; 
});


// Serviços Core
builder.Services.AddSingleton<EventLoader>();
builder.Services.AddSingleton<AnalyticsEngine>();
builder.Services.AddSingleton<FileTracker>();

// SQLite + Repository
builder.Services.AddDbContext<MetricsContext>(options =>
{
    options.UseSqlite("Data Source=metrics.db");
});
builder.Services.AddScoped<MetricsRepository>();

// Adicionado para evitar ciclos na serialização JSON
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// ---------------------------
// GET /stats
// ---------------------------
app.MapGet("/stats", (
    string? file,
    EventLoader eventLoader,
    AnalyticsEngine analyticsEngine,
    FileTracker tracker) =>
{
    var caminho = file ?? tracker.GetLastFile();

    if (caminho is null)
        return Results.BadRequest("Nenhum arquivo enviado ainda.");

    var eventos = eventLoader.LoadFromFile(caminho);
    var stats = analyticsEngine.CalculateStats(eventos);

    return Results.Ok(stats);
});

// ---------------------------
// POST /upload
// ---------------------------
app.MapPost("/upload", async (
    IFormFile file,
    EventLoader loader,
    AnalyticsEngine engine,
    FileTracker tracker,
    MetricsRepository repo) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("Arquivo inválido ou vazio.");

    Directory.CreateDirectory("uploads");

    // gera nome único
    var uniqueName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}_{file.FileName}";
    var filePath = Path.Combine("uploads", uniqueName);

    using (var stream = new FileStream(filePath, FileMode.Create))
        await file.CopyToAsync(stream);

    // processamento
    var eventos = loader.LoadFromFile(filePath);
    tracker.SetLastFile(filePath);
    var stats = engine.CalculateStats(eventos);

    // salva upload
    var upload = new UploadEntity
    {
        FilePath = filePath,
        Timestamp = DateTime.UtcNow
    };

    var uploadId = await repo.SaveUploadAsync(upload);

    // salva métricas por serviço
    foreach (var stat in stats)
    {
        var metric = new ServiceMetricEntity
        {
            UploadId = uploadId,
            ServiceName = stat.ServiceName,
            TotalRequests = stat.TotalRequests,
            SuccessCount = stat.SuccessCount,
            ErrorCount = stat.ErrorCount,
            SuccessRate = stat.SuccessRate,
            ErrorRate = stat.ErrorRate,
            AverageDurationMs = stat.AverageDurationMs,
            MinDuration = stat.MinDuration,
            MaxDuration = stat.MaxDuration
        };

        await repo.SaveMetricAsync(metric);
    }

    return Results.Created($"/history/{uploadId}", stats);
}).DisableAntiforgery();

// ---------------------------
// GET /stats/{serviceName}
// ---------------------------
app.MapGet("/stats/{serviceName}", (
    string serviceName,
    string? file,
    EventLoader loader,
    AnalyticsEngine engine,
    FileTracker tracker) =>
{
    var caminho = file ?? tracker.GetLastFile();

    if (caminho is null)
        return Results.BadRequest("Nenhum arquivo enviado ainda.");

    var eventos = loader.LoadFromFile(caminho);
    var stats = engine.CalculateStats(eventos);

    var statFiltrado = stats.FirstOrDefault(s => s.ServiceName == serviceName);

    if (statFiltrado is null)
        return Results.NotFound("Serviço não encontrado.");

    return Results.Ok(statFiltrado);
});

app.MapGet("/history", async(MetricsRepository repo) =>{
    var uploads = await repo.GetUploadsAsync();
    return Results.Ok(uploads);
});

app.MapGet("/history/{serviceName}", async (string serviceName, MetricsRepository repo) =>
{
    var metrics = await repo.GetMetricsByServiceAsync(serviceName);
    if (metrics.Count == 0)
    {
        return Results.NotFound("Serviço não encontrado no histórico");
    }
    return Results.Ok(metrics);
});



// ---------------------------
// cria BD se não existir
// ---------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MetricsContext>();
    db.Database.EnsureCreated();
}




app.Run();
