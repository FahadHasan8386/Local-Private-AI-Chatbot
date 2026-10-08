
using Local_Private_Ai_Chatbot.Api.Models;
using Local_Private_Ai_Chatbot.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Ollama Configuration

builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection("Ollama"));


// Ollama HttpClient

builder.Services.AddHttpClient<IOllamaChatService, OllamaChatService>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<OllamaOptions>>()
            .Value;

        client.BaseAddress = new Uri(options.BaseUrl);
    });

// Build application

var app = builder.Build();


// ==================================================
// Configure HTTP request pipeline
// ==================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();