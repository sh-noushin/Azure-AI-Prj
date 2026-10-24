using AiApp.Models;
using AiApp.Services;

namespace AiApp.Endpoints;

/// <summary>The /api/chat endpoint. Each feature gets its own file like this.</summary>
public static class ChatEndpoints
{
    public static void MapChat(this WebApplication app)
    {
        app.MapPost("/api/chat", async (ChatRequest request, ChatService chat) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return Results.BadRequest(new { error = "Bitte eine Frage eingeben." });
            }

            string answer = await chat.ContinueAsync(request.Question, request.History ?? []);
            return Results.Ok(new ChatResponse(answer));
        });
    }
}
