using Local_Private_Ai_Chatbot.Web;
using Local_Private_Ai_Chatbot.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// API HttpClient
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7005/")
    });


// Chat API Client
builder.Services.AddScoped<ChatApiClient>();


await builder.Build().RunAsync();