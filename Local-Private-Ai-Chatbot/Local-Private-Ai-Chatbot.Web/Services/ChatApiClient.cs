using Local_Private_Ai_Chatbot.Web.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;

namespace Local_Private_Ai_Chatbot.Web.Services;

public class ChatApiClient(HttpClient http)
{
    // Get installed Ollama models
    public async Task<List<string>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<List<string>>(
            "api/chat/models",
            cancellationToken) ?? [];
    }

    // Stream AI response from the API
    public async IAsyncEnumerable<string> StreamAsync(
        IEnumerable<ChatMessage> messages,
        string? model,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            messages = messages.Select(message => new
            {
                role = message.Role,
                content = message.Content
            }).ToList(),

            model = string.IsNullOrWhiteSpace(model)
                ? null
                : model
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/chat/stream")
        {
            Content = JsonContent.Create(requestBody)
        };

        // Enable streaming in Blazor WebAssembly
        request.SetBrowserResponseStreamingEnabled(true);

        using var response = await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        // Read error details instead of hiding them
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(
                cancellationToken);

            throw new HttpRequestException(
                $"API returned {(int)response.StatusCode} " +
                $"({response.ReasonPhrase}). Details: {error}");
        }

        // Read streamed response
        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        using var reader = new StreamReader(stream, Encoding.UTF8);

        var buffer = new char[256];
        int read;

        while ((read = await reader.ReadAsync(
            buffer.AsMemory(),
            cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return new string(buffer, 0, read);
        }
    }
}