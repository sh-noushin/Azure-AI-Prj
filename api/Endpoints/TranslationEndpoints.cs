using AiApp.Models;
using AiApp.Services;

namespace AiApp.Endpoints;

/// <summary>The /api/translate endpoint.</summary>
public static class TranslationEndpoints
{
    private const int MaxLength = 5000;

    public static void MapTranslation(this WebApplication app)
    {
        app.MapGet("/api/translate/languages", () => Results.Ok(TranslationService.Languages));

        app.MapPost("/api/translate", async (TranslateRequest request, TranslationService translator) =>
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.BadRequest(new { error = "Bitte einen Text eingeben." });
            }

            if (request.Text.Length > MaxLength)
            {
                return Results.BadRequest(new { error = $"Der Text ist zu lang (max. {MaxLength} Zeichen)." });
            }

            string target = string.IsNullOrWhiteSpace(request.To) ? "en" : request.To;
            return Results.Ok(await translator.TranslateAsync(request.Text, target));
        });
    }
}
