using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AiApp.Configuration;
using AiApp.Models;
using Azure.Core;
using Azure.Identity;

namespace AiApp.Services;

/// <summary>
/// Translates text. The Translator has no .NET SDK, so this calls its REST API directly
/// with the same az login token the other features use.
/// </summary>
public class TranslationService(AzureAiSettings settings, IHttpClientFactory httpClientFactory)
{
    private static readonly string[] Scope = ["https://cognitiveservices.azure.com/.default"];

    private readonly DefaultAzureCredential _credential = new();

    /// <summary>The languages offered in the dropdown.</summary>
    public static IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("en", "Englisch"),
        new("de", "Deutsch"),
        new("fr", "Französisch"),
        new("es", "Spanisch"),
        new("it", "Italienisch"),
        new("fa", "Persisch"),
        new("tr", "Türkisch")
    ];

    public async Task<TranslationResult> TranslateAsync(string text, string to)
    {
        HttpClient client = httpClientFactory.CreateClient();
        await AuthenticateAsync(client);

        string url = $"{settings.ServicesEndpoint.TrimEnd('/')}/translator/text/v3.0/translate?api-version=3.0&to={to}";
        HttpResponseMessage response = await client.PostAsJsonAsync(url, new[] { new { Text = text } });
        response.EnsureSuccessStatusCode();

        TranslatorReply[]? replies = await response.Content.ReadFromJsonAsync<TranslatorReply[]>();
        TranslatorReply? first = replies?.FirstOrDefault();

        return new TranslationResult(
            first?.Translations.FirstOrDefault()?.Text ?? "",
            first?.DetectedLanguage?.Language ?? "");
    }

    /// <summary>Adds either the API key or an az login token to the request.</summary>
    private async Task AuthenticateAsync(HttpClient client)
    {
        if (settings.UseApiKey)
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", settings.ApiKey);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", settings.Region);
            return;
        }

        AccessToken token = await _credential.GetTokenAsync(new TokenRequestContext(Scope), CancellationToken.None);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }

    /// <summary>The shape the Translator REST API answers with.</summary>
    private sealed record TranslatorReply(
        [property: JsonPropertyName("translations")] Translation[] Translations,
        [property: JsonPropertyName("detectedLanguage")] Detected? DetectedLanguage);

    private sealed record Translation([property: JsonPropertyName("text")] string Text);

    private sealed record Detected([property: JsonPropertyName("language")] string Language);
}
