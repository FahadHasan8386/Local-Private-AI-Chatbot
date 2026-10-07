using Local_Private_Ai_Chatbot.Api.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Local_Private_Ai_Chatbot.Api.Services;

public class OllamaChatService : IOllamaChatService
{
    private readonly HttpClient _http;
    private readonly OllamaOptions _options;

    public OllamaChatService( HttpClient http, OllamaOptions options)
    {
        _http = http;
        _options = options;
    }

    public async IAsyncEnumerable<string> ChatAsync(
        List<ChatMessageDto> messages,
        string? model,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new
        {
            model = model ?? _options.Model,
            messages,
            stream = true
        };

        using var response = await _http.PostAsJsonAsync(
            "api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var json = JsonDocument.Parse(line);

            if (json.RootElement.TryGetProperty(
                    "message",
                    out var message))
            {
                var content =
                    message.GetProperty("content").GetString();

                if (!string.IsNullOrEmpty(content))
                    yield return content;
            }

            if (json.RootElement.TryGetProperty(
                    "done",
                    out var done) &&
                done.GetBoolean())
            {
                yield break;
            }
        }
    }

    public async Task<List<string>> GetModelsAsync(
        CancellationToken cancellationToken)
    {
        var response = await _http.GetFromJsonAsync<JsonDocument>(
            "api/tags",
            cancellationToken);

        return response?
            .RootElement
            .GetProperty("models")
            .EnumerateArray()
            .Select(x => x.GetProperty("name").GetString()!)
            .ToList()
            ?? [];
    }
}
