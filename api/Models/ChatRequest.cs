namespace AiApp.Models;

/// <summary>
/// What the browser sends to /api/chat: the new question, plus the conversation so far
/// so the model can understand follow-up questions.
/// </summary>
public record ChatRequest(string Question, ChatTurn[]? History);
