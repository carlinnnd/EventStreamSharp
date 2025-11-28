using System.Net.Http.Json;

public class APIClient
{
    private readonly HttpClient _httpClient;

    public APIClient(string baseUrl)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = Timeout.InfiniteTimeSpan;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        return await _httpClient.GetFromJsonAsync<T>(endpoint);
    }

    public async Task<T?> PostFileAsync<T>(string endpoint, string filePath)
    {
        using var form = new MultipartFormDataContent();

        // Stream de verdade (não carrega o arquivo inteiro na memória)
        var fileStream = File.OpenRead(filePath);
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.Add("Content-Type", "application/octet-stream");

        form.Add(streamContent, "file", Path.GetFileName(filePath));

        var response = await _httpClient.PostAsync(endpoint, form);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }
}
