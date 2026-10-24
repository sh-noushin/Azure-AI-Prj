namespace AiApp.Models;

/// <summary>Which document is loaded and how many pieces it was cut into.</summary>
public record RagStatus(string DocumentName, int ChunkCount);
