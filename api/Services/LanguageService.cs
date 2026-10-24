using AiApp.Configuration;
using AiApp.Models;
using Azure;
using Azure.AI.TextAnalytics;
using Azure.Identity;

namespace AiApp.Services;

/// <summary>
/// Reads a piece of text and reports the mood, the main topics, the detected language
/// and the names it recognises.
/// </summary>
public class LanguageService
{
    private readonly TextAnalyticsClient _client;

    public LanguageService(AzureAiSettings settings)
    {
        _client = settings.UseApiKey
            ? new TextAnalyticsClient(new Uri(settings.ServicesEndpoint), new AzureKeyCredential(settings.ApiKey))
            : new TextAnalyticsClient(new Uri(settings.ServicesEndpoint), new DefaultAzureCredential());
    }

    public async Task<TextAnalysisResult> AnalyseAsync(string text)
    {
        DocumentSentiment sentiment = await _client.AnalyzeSentimentAsync(text);
        var detected = await _client.DetectLanguageAsync(text);
        var phrases = await _client.ExtractKeyPhrasesAsync(text);
        var entities = await _client.RecognizeEntitiesAsync(text);

        return new TextAnalysisResult(
            sentiment.Sentiment.ToString().ToLowerInvariant(),
            Math.Round(sentiment.ConfidenceScores.Positive, 3),
            Math.Round(sentiment.ConfidenceScores.Neutral, 3),
            Math.Round(sentiment.ConfidenceScores.Negative, 3),
            detected.Value.Name,
            phrases.Value.ToList(),
            entities.Value.Select(e => new NamedEntity(e.Text, e.Category.ToString())).ToList());
    }
}
