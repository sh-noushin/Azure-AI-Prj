namespace AiApp.Models;

/// <summary>Text to read out loud, and which voice should do it.</summary>
public record SpeakRequest(string Text, string? Voice);
