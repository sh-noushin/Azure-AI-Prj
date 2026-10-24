using AiApp.Models;
using AiApp.Services;

namespace AiApp.Endpoints;

/// <summary>The /api/similarity endpoint.</summary>
public static class EmbeddingEndpoints
{
    public static void MapEmbeddings(this WebApplication app)
    {
        app.MapPost("/api/similarity", async (SimilarityRequest request, EmbeddingService embeddings) =>
        {
            if (string.IsNullOrWhiteSpace(request.First) || string.IsNullOrWhiteSpace(request.Second))
            {
                return Results.BadRequest(new { error = "Bitte beide Texte eingeben." });
            }

            float[] first = await embeddings.EmbedAsync(request.First);
            float[] second = await embeddings.EmbedAsync(request.Second);

            double similarity = EmbeddingService.CosineSimilarity(first, second);
            return Results.Ok(new SimilarityResponse(Math.Round(similarity, 4), first.Length));
        });
    }
}
