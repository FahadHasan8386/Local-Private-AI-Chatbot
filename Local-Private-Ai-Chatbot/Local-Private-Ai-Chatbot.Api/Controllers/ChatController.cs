using Microsoft.AspNetCore.Mvc;

namespace Local_Private_Ai_Chatbot.Api.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
