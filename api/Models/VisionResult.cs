namespace AiApp.Models;

/// <summary>
/// What we found in the picture: a description written by the chat model,
/// the text read out of it, and the things the Vision service recognised.
/// </summary>
public record VisionResult(
    string Description,
    IReadOnlyList<string> TextLines,
    IReadOnlyList<VisionTag> Tags);
