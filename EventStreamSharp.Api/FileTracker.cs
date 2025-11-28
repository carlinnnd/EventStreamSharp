using EventStreamSharp.Ingest;
using EventStreamSharp.Processing;
using Microsoft.AspNetCore.Http;

namespace EventStreamSharp.API;

public class FileTracker
{
    private string? _lastFile;

    public void SetLastFile(string filePath)
    {
        _lastFile = filePath;
    }

    public string? GetLastFile()
    {
        return _lastFile;
    }
}
