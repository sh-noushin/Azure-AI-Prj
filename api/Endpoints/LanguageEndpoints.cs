using AiApp.Models;
using AiApp.Services;

namespace AiApp.Endpoints;

/// <summary>The /api/language endpoint.</summary>
public static class LanguageEndpoints
{
    /// <summary>The Language service accepts at most this much text in one call.</summary>
    private const int MaxLength = 5000;

    public static void MapLanguage(this WebApplication app)
    {
        app.MapPost("/api/language", async (TextAnalysisRequest request, LanguageService language) =>
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.BadRequest(new { error = "Bitte einen Text eingeben." });
            }

            if (request.Text.Length > MaxLength)
            {
                return Results.BadRequest(new { error = $"Der Text ist zu lang (max. {MaxLength} Zeichen)." });
            }

            return Results.Ok(await language.AnalyseAsync(request.Text));
        });
    }
}
