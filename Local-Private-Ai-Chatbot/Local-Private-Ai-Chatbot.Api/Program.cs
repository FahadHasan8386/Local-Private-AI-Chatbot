using Local_Private_Ai_Chatbot.Api.Models;
using Local_Private_Ai_Chatbot.Api.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Ollama configuration
builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection("Ollama"));


// Ollama service
builder.Services.AddHttpClient<
    IOllamaChatService,
    OllamaChatService>((serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<OllamaOptions>>()
            .Value;

        client.BaseAddress = new Uri(options.BaseUrl);
    });


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:7006")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("BlazorClient");

app.UseAuthorization();

app.MapControllers();

app.Run();