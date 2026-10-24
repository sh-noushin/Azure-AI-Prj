using AiApp.Configuration;
using AiApp.Models;
using Azure.Core;
using Azure.Identity;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AiApp.Services;

/// <summary>Turns text into spoken audio, and spoken audio back into text.</summary>
public class SpeechService(AzureAiSettings settings)
{
    private static readonly string[] Scope = ["https://cognitiveservices.azure.com/.default"];

    private readonly DefaultAzureCredential _credential = new();

    /// <summary>The voices offered in the dropdown.</summary>
    public static IReadOnlyList<VoiceOption> Voices { get; } =
    [
        new("de-DE-KatjaNeural", "Katja (Deutsch)"),
        new("de-DE-ConradNeural", "Conrad (Deutsch)"),
        new("en-US-JennyNeural", "Jenny (English)"),
        new("en-GB-RyanNeural", "Ryan (English)")
    ];

    /// <summary>Reads the text out loud and returns it as MP3 bytes.</summary>
    public async Task<byte[]> TextToSpeechAsync(string text, string? voice)
    {
        SpeechConfig config = await CreateConfigAsync();
        config.SpeechSynthesisVoiceName = voice ?? Voices[0].Id;
        config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz48KBitRateMonoMp3);

        using var synthesizer = new SpeechSynthesizer(config, null);
        using SpeechSynthesisResult result = await synthesizer.SpeakTextAsync(text);

        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            throw new InvalidOperationException(Describe(SpeechSynthesisCancellationDetails.FromResult(result)));
        }

        return result.AudioData;
    }

    /// <summary>Listens to a WAV file and writes down what was said.</summary>
    public async Task<TranscriptResult> SpeechToTextAsync(Stream wavAudio, string language)
    {
        SpeechConfig config = await CreateConfigAsync();
        config.SpeechRecognitionLanguage = language;

        using var input = AudioInputStream.CreatePushStream();
        using var audio = AudioConfig.FromStreamInput(input);

        await CopyToAsync(wavAudio, input);

        using var recognizer = new SpeechRecognizer(config, audio);
        SpeechRecognitionResult result = await recognizer.RecognizeOnceAsync();

        return result.Reason switch
        {
            ResultReason.RecognizedSpeech => new TranscriptResult(result.Text, language),
            ResultReason.NoMatch => new TranscriptResult("", language),
            _ => throw new InvalidOperationException(
                CancellationDetails.FromResult(result).ErrorDetails ?? "Die Erkennung ist fehlgeschlagen.")
        };
    }

    /// <summary>
    /// Builds the connection. The custom subdomain lets the Speech service accept the same
    /// az login token as the other features, so no API key is needed.
    /// </summary>
    private async Task<SpeechConfig> CreateConfigAsync()
    {
        AccessToken token = await _credential.GetTokenAsync(new TokenRequestContext(Scope), CancellationToken.None);
        SpeechConfig config = SpeechConfig.FromEndpoint(new Uri(settings.ServicesEndpoint));
        config.AuthorizationToken = token.Token;

        return config;
    }

    private static async Task CopyToAsync(Stream source, PushAudioInputStream target)
    {
        byte[] buffer = new byte[8192];
        int read;

        while ((read = await source.ReadAsync(buffer)) > 0)
        {
            target.Write(buffer, read);
        }

        target.Close();
    }

    private static string Describe(SpeechSynthesisCancellationDetails details) =>
        string.IsNullOrWhiteSpace(details.ErrorDetails)
            ? "Die Sprachausgabe ist fehlgeschlagen."
            : details.ErrorDetails;
}
