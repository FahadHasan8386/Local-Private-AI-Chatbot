using Local_Private_Ai_Chatbot.Web.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;

namespace Local_Private_Ai_Chatbot.Web.Services;

public class ChatApiClient(HttpClient http)
{
    // Get available Ollama models

    public async Task<List<string>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await http.GetFromJsonAsync<List<string>>(
            "api/chat/models",
            cancellationToken);

        return models ?? [];
    }


    // Stream AI response
    public async IAsyncEnumerable<string> StreamAsync( IEnumerable<ChatMessage> messages,
        string? model, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Create request body
        var requestBody = new
        {
            messages = messages.Select(message => new
            {
                role = message.Role,
                content = message.Content
            }),

            model = string.IsNullOrWhiteSpace(model)
                ? null
                : model
        };


        // Create HTTP request
        using var request = new HttpRequestMessage( HttpMethod.Post,"api/chat/stream")
        {
            Content = JsonContent.Create(requestBody)
        };


        // Required for response streaming in Blazor WebAssembly
        request.SetBrowserResponseStreamingEnabled(true);


        // Send request
        using var response = await http.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);


        // Throw exception if API returns 4xx/5xx
        response.EnsureSuccessStatusCode();


        // Read response stream
        await using var stream = await response.Content.ReadAsStreamAsync( cancellationToken);

        using var reader = new StreamReader( stream, Encoding.UTF8);


        // Read chunks from API
        var buffer = new char[256];

        int read;

        while ((read = await reader.ReadAsync(
            buffer.AsMemory(),
            cancellationToken)) > 0)
        {
            yield return new string(
                buffer,
                0,
                read);
        }
    }
}
