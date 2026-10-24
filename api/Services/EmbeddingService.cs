using AiApp.Configuration;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Embeddings;

namespace AiApp.Services;

/// <summary>
/// Turns text into numbers (a "vector") so we can measure how similar two texts are
/// in meaning, not just in spelling.
/// </summary>
public class EmbeddingService
{
    private readonly EmbeddingClient _embeddings;

    public EmbeddingService(AzureAiSettings settings)
    {
        AzureOpenAIClient azure = settings.UseApiKey
            ? new(new Uri(settings.OpenAiEndpoint), new AzureKeyCredential(settings.ApiKey))
            : new(new Uri(settings.OpenAiEndpoint), new DefaultAzureCredential());

        _embeddings = azure.GetEmbeddingClient(settings.EmbeddingDeployment);
    }

    /// <summary>Converts one text into its vector.</summary>
    public async Task<float[]> EmbedAsync(string text)
    {
        var result = await _embeddings.GenerateEmbeddingAsync(text);
        return result.Value.ToFloats().ToArray();
    }

    /// <summary>How close two texts are in meaning: 1.0 = the same, 0.0 = unrelated.</summary>
    public async Task<double> SimilarityAsync(string first, string second)
    {
        float[] a = await EmbedAsync(first);
        float[] b = await EmbedAsync(second);
        return CosineSimilarity(a, b);
    }

    /// <summary>
    /// Cosine similarity: the angle between the two vectors.
    /// Pointing the same way means the texts mean the same thing.
    /// </summary>
    public static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, lengthA = 0, lengthB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            lengthA += a[i] * a[i];
            lengthB += b[i] * b[i];
        }

        return dot / (Math.Sqrt(lengthA) * Math.Sqrt(lengthB));
    }
}
