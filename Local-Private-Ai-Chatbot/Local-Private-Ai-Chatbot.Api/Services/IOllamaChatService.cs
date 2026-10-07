using Local_Private_Ai_Chatbot.Api.Models;

namespace Local_Private_Ai_Chatbot.Api.Services;

public interface IOllamaChatService
{
    IAsyncEnumerable<string> ChatAsync(List<ChatMessageDto> messages,string? model,CancellationToken cancellationToken);

    Task<List<string>> GetModelsAsync(CancellationToken cancellationToken);
}
