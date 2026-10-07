namespace Local_Private_Ai_Chatbot.Api.Models;

public record ChatRequest(List<ChatMessageDto> Message, string? Model);
