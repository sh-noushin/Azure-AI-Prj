using AiApp.Services;

namespace AiApp.Endpoints;

/// <summary>The /api/vision endpoint: upload a picture, get a description, its text and tags.</summary>
public static class VisionEndpoints
{
    private const long MaxFileSize = 4 * 1024 * 1024;

    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    public static void MapVision(this WebApplication app)
    {
        app.MapPost("/api/vision", async (IFormFile file, VisionService vision) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest(new { error = "Die Datei ist leer." });
            }

            if (file.Length > MaxFileSize)
            {
                return Results.BadRequest(new { error = "Das Bild ist zu gross (max. 4 MB)." });
            }

            if (!AllowedTypes.Contains(file.ContentType))
            {
                return Results.BadRequest(new { error = "Bitte ein JPEG-, PNG-, WebP- oder GIF-Bild wählen." });
            }

            using var memory = new MemoryStream();
            await file.CopyToAsync(memory);

            BinaryData image = BinaryData.FromBytes(memory.ToArray());
            return Results.Ok(await vision.AnalyseAsync(image, file.ContentType));
        })
        .DisableAntiforgery();
    }
}
