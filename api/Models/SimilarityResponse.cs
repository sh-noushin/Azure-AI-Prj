namespace AiApp.Models;

/// <summary>How similar the two texts are, plus how many numbers each vector has.</summary>
public record SimilarityResponse(double Similarity, int Dimensions);
