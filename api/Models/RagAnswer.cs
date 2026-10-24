namespace AiApp.Models;

/// <summary>The answer, plus the pieces of the document it came from.</summary>
public record RagAnswer(string Answer, IReadOnlyList<string> Sources);
