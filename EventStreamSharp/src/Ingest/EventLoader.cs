using System;
using System.Collections.Generic;
using System.IO;
using EventStreamSharp.Domain;
namespace EventStreamSharp.Ingest;
public class EventLoader
{
    private readonly EventParser _parser = new EventParser();
    public List<EventRecord> LoadFromFile(string filePath)
    {
        if (!File.Exists (filePath))
        {
            Console.WriteLine($"[WARN]Arquivo não encontrado: {filePath}");
            return new List<EventRecord>();
        }
        var eventos = new List<EventRecord>();
        var lineCount = 0;
        try{
            foreach (var linha in File.ReadLines(filePath))
            {
                lineCount++;
                var linhaLimpa = linha.Trim();
                if (string.IsNullOrEmpty(linhaLimpa)) continue;

                var evento = _parser.Parse(linhaLimpa);
                if (evento != null)
                {
                    eventos.Add(evento);
                }
            }
            Console.WriteLine($"[INFO] Processado '{filePath}': {lineCount} linhas lidas, {eventos.Count} eventos parseados.");
            return eventos;
        }
        catch(IOException ex)
        {
            Console.WriteLine($"[ERROR]Erro ao ler o arquivo: {ex.Message}");
           
        }
        catch(UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[ERROR]Acesso negado ao arquivo: {ex.Message}");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[ERROR]Erro inesperado: {ex.Message}");
        }

        return new List<EventRecord>();
    }
}
