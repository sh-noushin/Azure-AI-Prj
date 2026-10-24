namespace AiApp.Models;

/// <summary>One message in the conversation. Role is "user" or "assistant".</summary>
public record ChatTurn(string Role, string Text);
