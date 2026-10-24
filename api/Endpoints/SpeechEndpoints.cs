using AiApp.Models;
using AiApp.Services;

namespace AiApp.Endpoints;

/// <summary>Text to speech, and speech to text.</summary>
public static class SpeechEndpoints
{
    private const int MaxTextLength = 1000;

    private const long MaxAudioSize = 10 * 1024 * 1024;

    public static void MapSpeech(this WebApplication app)
    {
        app.MapGet("/api/speech/voices", () => Results.Ok(SpeechService.Voices));

        app.MapPost("/api/speech/speak", async (SpeakRequest request, SpeechService speech) =>
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.BadRequest(new { error = "Bitte einen Text eingeben." });
            }

            if (request.Text.Length > MaxTextLength)
            {
                return Results.BadRequest(new { error = $"Der Text ist zu lang (max. {MaxTextLength} Zeichen)." });
            }

            byte[] audio = await speech.TextToSpeechAsync(request.Text, request.Voice);
            return Results.File(audio, "audio/mpeg");
        });

        app.MapPost("/api/speech/transcribe", async (IFormFile file, string? language, SpeechService speech) =>
        {
            if (file.Length == 0 || file.Length > MaxAudioSize)
            {
                return Results.BadRequest(new { error = "Bitte eine WAV-Datei bis 10 MB wählen." });
            }

            using Stream audio = file.OpenReadStream();
            TranscriptResult result = await speech.SpeechToTextAsync(audio, language ?? "de-DE");

            return Results.Ok(result);
        })
        .DisableAntiforgery();
    }
}
