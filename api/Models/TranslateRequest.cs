namespace AiApp.Models;

/// <summary>Text to translate, and the language to translate it into.</summary>
public record TranslateRequest(string Text, string To);
