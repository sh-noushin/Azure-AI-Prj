namespace AiApp.Models;

/// <summary>The translated text, plus the language the original was written in.</summary>
public record TranslationResult(string Text, string DetectedLanguage);
