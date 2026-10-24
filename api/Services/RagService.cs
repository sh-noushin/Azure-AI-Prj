using AiApp.Models;

namespace AiApp.Services;

/// <summary>
/// Answers questions about a document you upload.
/// Steps: cut the text into pieces, turn each piece into numbers, find the pieces
/// closest to the question, and let the chat model answer using only those pieces.
/// </summary>
public class RagService(EmbeddingService embeddings, ChatService chat)
{
    /// <summary>Roughly how many characters one piece of text should have.</summary>
    private const int ChunkSize = 700;

    /// <summary>How many pieces are given to the model as context.</summary>
    private const int TopPieces = 3;

    private readonly List<DocumentChunk> _chunks = [];

    public string DocumentName { get; private set; } = "";

    public int ChunkCount => _chunks.Count;

    /// <summary>Reads a document and prepares it for questions.</summary>
    public async Task<int> LoadAsync(string fileName, string content)
    {
        _chunks.Clear();
        DocumentName = fileName;

        foreach (string piece in SplitIntoPieces(content))
        {
            float[] vector = await embeddings.EmbedAsync(piece);
            _chunks.Add(new DocumentChunk(piece, vector));
        }

        return _chunks.Count;
    }

    /// <summary>Answers a question using only the uploaded document.</summary>
    public async Task<RagAnswer> AskAsync(string question)
    {
        if (_chunks.Count == 0)
        {
            return new RagAnswer("Bitte laden Sie zuerst ein Dokument hoch.", []);
        }

        float[] questionVector = await embeddings.EmbedAsync(question);

        List<DocumentChunk> best = _chunks
            .OrderByDescending(c => EmbeddingService.CosineSimilarity(questionVector, c.Vector))
            .Take(TopPieces)
            .ToList();

        string context = string.Join("\n\n---\n\n", best.Select(c => c.Text));

        string instruction =
            "Du beantwortest Fragen ausschliesslich mit dem folgenden Text. "
            + "Steht die Antwort nicht darin, sage genau: 'Das steht nicht im Dokument.' "
            + "Erfinde nichts.\n\nTEXT:\n" + context;

        string answer = await chat.AskAsync(instruction, question);

        return new RagAnswer(answer, best.Select(c => Preview(c.Text)).ToList());
    }

    /// <summary>Cuts the text into pieces at blank lines, keeping pieces roughly equal in size.</summary>
    private static List<string> SplitIntoPieces(string content)
    {
        var pieces = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (string paragraph in content.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.Length + paragraph.Length > ChunkSize && current.Length > 0)
            {
                pieces.Add(current.ToString().Trim());
                current.Clear();
            }

            current.AppendLine(paragraph.Trim());
        }

        if (current.Length > 0)
        {
            pieces.Add(current.ToString().Trim());
        }

        return pieces;
    }

    private static string Preview(string text) =>
        text.Length <= 160 ? text : text[..160] + " …";
}
