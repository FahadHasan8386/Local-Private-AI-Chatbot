
using Local_Private_Ai_Chatbot.Api.Models;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Local_Private_Ai_Chatbot.Api.Services;

public class OllamaChatService : IOllamaChatService
{
    private readonly HttpClient _http;
    private readonly OllamaOptions _options;

    public OllamaChatService(HttpClient http,IOptions<OllamaOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async IAsyncEnumerable<string> ChatAsync(
        List<ChatMessageDto> messages,
        string? model,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var selectedModel = string.IsNullOrWhiteSpace(model)
            || model == "string"? _options.Model: model;

        var request = new
        {
            model = selectedModel,
            messages = messages.Select(m => new
            {
                role = m.Role,
                content = m.Content
            }),
            stream = true
        };

        using var response = await _http.PostAsJsonAsync(
            "api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var json = JsonDocument.Parse(line);
            var root = json.RootElement;

            if (root.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var content))
            {
                var text = content.GetString();

                if (!string.IsNullOrEmpty(text))
                    yield return text;
            }

            if (root.TryGetProperty("done", out var done) &&
                done.GetBoolean())
            {
                yield break;
            }
        }
    }

    public async Task<List<string>> GetModelsAsync(
        CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(
            "api/tags",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        return document.RootElement
            .GetProperty("models")
            .EnumerateArray()
            .Select(m => m.GetProperty("name").GetString())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();
    }
}