using Spectre.Console;
using EventStreamSharp.Dashboard.Models;
using System.ComponentModel.DataAnnotations.Schema;

var client = new APIClient("http://localhost:5015");

await ShowHistory();
await LoadAllCsvFromFolder(@"C:\treinoC#\Eventos");



async Task ShowHistory()
{
    AnsiConsole.MarkupLine("[green]Carregando histórico...[/]");

    var uploads = await client.GetAsync<List<UploadInfo>>("/history");

    if (uploads is null || uploads.Count == 0)
    {
        AnsiConsole.MarkupLine("[red]Nenhum upload encontrado.[/]");
        return;
    }

    var table = new Table();
    table.Border = TableBorder.Rounded;
    table.AddColumn("ID");
    table.AddColumn("Arquivo");
    table.AddColumn("Data");
    table.AddColumn("Serviços");
    foreach(var u in uploads)
    {

        table.AddRow(
            u.Id.ToString(),
            u.FilePath,
            u.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            u.Metrics.Count.ToString()
        );}
        AnsiConsole.Write(table);
    var services = uploads
    .SelectMany(u => u.Metrics.Select(m => m.ServiceName))
    .Distinct()
    .OrderBy(s => s)
    .ToList();
    var service = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Selecione um [green]serviço[/] para ver o histórico:")
        .AddChoices(services)
    );
    await ShowServiceHistory(service);
}
async Task ShowServiceHistory(string serviceName)
{
    AnsiConsole.MarkupLine($"[yellow]Carregando histórico do serviço:[/] [green]{serviceName}[/]");

    var metrics = await client.GetAsync<List<ServiceMetric>>($"/history/{serviceName}");

    if (metrics is null || metrics.Count == 0)
    {
        AnsiConsole.MarkupLine($"[red]Nenhum histórico encontrado para {serviceName}[/]");
        return;
    }

    string Bar(double value) =>
        new string('█', (int)(value / 10)).PadRight(10, '░');

    var table = new Table();
    table.Border = TableBorder.Rounded;

    table.AddColumn("Upload");
    table.AddColumn("Success");
    table.AddColumn("Error");
    table.AddColumn("Avg Duration");
    table.AddColumn("Graph");

    foreach (var m in metrics)
    {
        var bar = Bar(m.SuccessRate);

        table.AddRow(
            m.UploadId.ToString(),
            $"{m.SuccessRate:F1}%",
            $"{m.ErrorRate:F1}%",
            $"{m.AverageDurationMs:F0} ms",
            $"[green]{bar}[/]"
        );
    }

    AnsiConsole.Write(table);
}

async Task LoadAllCsvFromFolder(string folderPath)
{
    if (!Directory.Exists(folderPath))
    {
        AnsiConsole.MarkupLine($"[red]Pasta não encontrada:[/] {folderPath}");
        return;
    }

    var files = Directory.GetFiles(folderPath, "*.csv");

    if (files.Length == 0)
    {
        AnsiConsole.MarkupLine("[red]Nenhum CSV encontrado na pasta.[/]");
        return;
    }

    foreach (var file in files)
    {
        AnsiConsole.MarkupLine($"[yellow]Enviando:[/] {file}");

        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(file);

        content.Add(new StreamContent(fileStream), "file", Path.GetFileName(file));

        var response = await new HttpClient().PostAsync("http://localhost:5015/upload", content);

        if (response.IsSuccessStatusCode)
            AnsiConsole.MarkupLine("[green]✔ Enviado com sucesso[/]");
        else
            AnsiConsole.MarkupLine("[red]✘ Falhou[/]");
    }
}
