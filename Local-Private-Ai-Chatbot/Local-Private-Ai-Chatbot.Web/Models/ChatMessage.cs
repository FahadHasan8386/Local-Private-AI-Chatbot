namespace Local_Private_Ai_Chatbot.Web.Models;

public class ChatMessage
{
    public string Role { get; set; }

    public string Content { get; set; }

    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}