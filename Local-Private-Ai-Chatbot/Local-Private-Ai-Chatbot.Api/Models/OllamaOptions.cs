namespace Local_Private_Ai_Chatbot.Api.Models
{
    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:11434/";
        public string Model { get; set; } = "llama3.2";
        public string? SystemPrompt { get; set; }
    }
}
