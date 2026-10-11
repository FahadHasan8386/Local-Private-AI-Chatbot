
using Local_Private_Ai_Chatbot.Api.Models;
using Local_Private_Ai_Chatbot.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ollama configuration
builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection("Ollama"));

// Ollama HTTP client
builder.Services.AddHttpClient<IOllamaChatService, OllamaChatService>(
    (sp, client) =>
    {
        var options = sp
            .GetRequiredService<IOptions<OllamaOptions>>()
            .Value;

        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromMinutes(10);
    });

// CORS: use your actual Blazor frontend URL
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:7005")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("BlazorClient");
app.UseAuthorization();

app.MapControllers();

app.Run();
