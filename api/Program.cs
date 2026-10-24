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
builder.Services.AddSingleton<LanguageService>();
builder.Services.AddSingleton<VisionService>();
builder.Services.AddSingleton<SpeechService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<TranslationService>();

// ---------- let the Angular dev server call us ----------
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

// ---------- endpoints (one per feature) ----------
// A root page, so opening http://localhost:5080 in a browser is not a 404.
app.MapGet("/", () => Results.Ok(new
{
    name = "Azure AI Playground API",
    website = "http://localhost:4200",
    endpoints = new[]
    {
        "POST /api/chat",
        "POST /api/similarity",
        "POST /api/rag/upload", "GET /api/rag/status", "POST /api/rag/ask",
        "POST /api/language",
        "POST /api/vision",
        "GET /api/speech/voices", "POST /api/speech/speak", "POST /api/speech/transcribe",
        "GET /api/translate/languages", "POST /api/translate",
        "POST /api/voice/ask"
    }
}));

app.MapChat();
app.MapEmbeddings();
app.MapRag();
app.MapLanguage();
app.MapVision();
app.MapSpeech();
app.MapTranslation();
app.MapVoice();

app.Run();
