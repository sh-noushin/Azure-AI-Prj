using AiApp.Configuration;
using AiApp.Endpoints;
using AiApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- settings ----------
// ASP.NET Core reads appsettings.json and appsettings.Development.json by itself.
AzureAiSettings settings = builder.Configuration.GetSection("AzureAi").Get<AzureAiSettings>()
    ?? throw new InvalidOperationException("Section 'AzureAi' is missing from appsettings.json.");

builder.Services.AddSingleton(settings);

// ---------- services (one per Azure AI feature) ----------
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<RagService>();

// ---------- let the Angular dev server call us ----------
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

// ---------- endpoints (one per feature) ----------
app.MapChat();
app.MapEmbeddings();
app.MapRag();

app.Run();
