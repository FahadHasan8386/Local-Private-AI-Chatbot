using Local_Private_Ai_Chatbot.Api.Models;
using Local_Private_Ai_Chatbot.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Local_Private_Ai_Chatbot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IOllamaChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IOllamaChatService chatService,ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// Streams the AI assistant response as plain text chunks.
    [HttpPost("stream")]
    public async Task Stream([FromBody] ChatRequest request,CancellationToken cancellationToken)
    {
        // Validate request
        if (request.Messages == null || request.Messages.Count == 0)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;

            await Response.WriteAsync(
                "Messages are required.",
                cancellationToken);

            return;
        }

        // Tell the client that response will be streamed
        Response.ContentType = "text/plain; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";

        try
        {
            // Get AI response chunk by chunk
            await foreach (var chunk in _chatService.ChatAsync(
                request.Messages,
                request.Model,
                cancellationToken))
            {
                await Response.WriteAsync(
                    chunk,
                    cancellationToken);

                // Send the chunk immediately
                await Response.Body.FlushAsync(
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected or cancelled the request.
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to communicate with Ollama.");

            // If response has not started,
            // we can still change the HTTP status code.
            if (!Response.HasStarted)
            {
                Response.StatusCode =
                    StatusCodes.Status502BadGateway;
            }

            await Response.WriteAsync(
                "\n[Could not reach Ollama. Is Ollama running?]");
        }
    }


    /// Returns the Ollama models installed locally.
    [HttpGet("models")]
    public async Task<ActionResult<List<string>>> GetModels(
        CancellationToken cancellationToken)
    {
        try
        {
            var models = await _chatService.GetModelsAsync(
                cancellationToken);

            return Ok(models);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to get Ollama models.");

            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Could not reach Ollama.");
        }
    }
}
