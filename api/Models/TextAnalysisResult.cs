namespace AiApp.Models;

/// <summary>What the Language service found in the text.</summary>
public record TextAnalysisResult(
    string Sentiment,
    double Positive,
    double Neutral,
    double Negative,
    string Language,
    IReadOnlyList<string> KeyPhrases,
    IReadOnlyList<NamedEntity> Entities);
