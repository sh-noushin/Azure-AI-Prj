namespace AiApp.Services;

/// <summary>One piece of the uploaded document, together with its vector.</summary>
public record DocumentChunk(string Text, float[] Vector);
