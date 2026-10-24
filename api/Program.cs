using AiApp.Configuration;
using AiApp.Endpoints;
using AiApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- settings ----------
// ASP.NET Core reads appsettings.json and appsettings.Development.json by itself.
AzureAiSettings settings = builder.Configuration.GetSection("AzureAi").Get<AzureAiSettings>()
    ?? throw new InvalidOperationException("Section 'AzureAi' is missing from appsettings.json.");

// Fail at startup with a clear message rather than on the first request with a 401.
if (string.IsNullOrWhiteSpace(settings.OpenAiEndpoint) || string.IsNullOrWhiteSpace(settings.ServicesEndpoint))
{
    throw new InvalidOperationException(
        "Azure endpoints are not configured. Copy api/appsettings.Development.Example.json to "
        + "api/appsettings.Development.json and fill in your own values, or set AZURE_OPENAI_ENDPOINT "
        + "and AZURE_SERVICES_ENDPOINT in .env when running with Docker.");
}

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
// Only needed while developing. In Docker the browser talks to nginx, which
// forwards /api internally, so there is no cross-origin request at all.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins("http://localhost:4300", "http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

// ---------- endpoints (one per feature) ----------
// A root page, so opening http://localhost:5080 in a browser is not a 404.
app.MapGet("/", () => Results.Ok(new
{
    name = "Azure AI Playground API",
    website = "http://localhost:4300",
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
