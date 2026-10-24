namespace AiApp.Models;

/// <summary>
/// One turn of a spoken conversation: what we heard, what the model answered,
/// and that answer as audio the browser can play.
/// </summary>
public record VoiceReply(string Question, string Answer, string AudioBase64);
