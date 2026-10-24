using AiApp.Models;
using AiApp.Services;

namespace AiApp.Endpoints;

/// <summary>
/// Speak a question and hear the answer. Three services in one call:
/// Speech writes down what you said, the chat model answers, Speech reads the answer back.
/// </summary>
public static class VoiceEndpoints
{
    private const long MaxAudioSize = 10 * 1024 * 1024;

    /// <summary>Spoken answers should stay short, so the model is told to keep it brief.</summary>
    private const string Instruction =
        "Du bist ein hilfsbereiter Assistent. Antworte in hoechstens drei kurzen Saetzen, "
        + "denn deine Antwort wird vorgelesen. Keine Aufzaehlungen, keine Formatierung.";

    public static void MapVoice(this WebApplication app)
    {
        app.MapPost("/api/voice/ask", async (
            IFormFile file,
            string? language,
            string? voice,
            SpeechService speech,
            ChatService chat) =>
        {
            if (file.Length == 0 || file.Length > MaxAudioSize)
            {
                return Results.BadRequest(new { error = "Bitte eine WAV-Aufnahme bis 10 MB senden." });
            }

            using Stream audio = file.OpenReadStream();
            TranscriptResult heard = await speech.SpeechToTextAsync(audio, language ?? "de-DE");

            if (string.IsNullOrWhiteSpace(heard.Text))
            {
                return Results.Ok(new VoiceReply("", "Ich habe nichts verstanden. Bitte noch einmal.", ""));
            }

            string answer = await chat.AskAsync(Instruction, heard.Text);
            byte[] spoken = await speech.TextToSpeechAsync(answer, voice);

            return Results.Ok(new VoiceReply(heard.Text, answer, Convert.ToBase64String(spoken)));
        })
        .DisableAntiforgery();
    }
}
