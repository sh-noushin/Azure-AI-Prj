using AiApp.Models;
using AiApp.Services;

namespace AiApp.Endpoints;

/// <summary>Upload a document, then ask questions about it.</summary>
public static class RagEndpoints
{
    /// <summary>Biggest file we accept, to keep embedding costs small.</summary>
    private const long MaxFileSize = 4 * 1024 * 1024;

    public static void MapRag(this WebApplication app)
    {
        app.MapPost("/api/rag/upload", async (IFormFile file, RagService rag) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest(new { error = "Die Datei ist leer." });
            }

            if (file.Length > MaxFileSize)
            {
                return Results.BadRequest(new { error = "Die Datei ist zu gross (max. 4 MB)." });
            }

            if (!DocumentReader.IsSupported(file.FileName))
            {
                return Results.BadRequest(new { error = "Bitte eine PDF-, TXT- oder Markdown-Datei wählen." });
            }

            using Stream upload = file.OpenReadStream();
            string content = await DocumentReader.ReadAsync(upload, file.FileName);

            if (string.IsNullOrWhiteSpace(content))
            {
                return Results.BadRequest(new
                {
                    error = "Aus der Datei liess sich kein Text lesen. "
                        + "Gescannte PDFs enthalten nur Bilder: probieren Sie die Bildanalyse."
                });
            }

            int pieces = await rag.LoadAsync(file.FileName, content);
            return Results.Ok(new RagStatus(file.FileName, pieces));
        })
        .DisableAntiforgery();

        app.MapGet("/api/rag/status", (RagService rag) =>
            Results.Ok(new RagStatus(rag.DocumentName, rag.ChunkCount)));

        app.MapPost("/api/rag/ask", async (ChatRequest request, RagService rag) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return Results.BadRequest(new { error = "Bitte eine Frage eingeben." });
            }

            return Results.Ok(await rag.AskAsync(request.Question));
        });
    }
}
