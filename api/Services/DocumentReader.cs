using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AiApp.Services;

/// <summary>
/// Gets the plain text out of an uploaded file. Text and Markdown are read as they are;
/// a PDF stores its words as drawing instructions, so they have to be extracted.
/// </summary>
public static class DocumentReader
{
    public static bool IsSupported(string fileName) =>
        Extension(fileName) is ".txt" or ".md" or ".markdown" or ".pdf";

    public static async Task<string> ReadAsync(Stream file, string fileName)
    {
        if (Extension(fileName) != ".pdf")
        {
            using var reader = new StreamReader(file);
            return await reader.ReadToEndAsync();
        }

        // PdfPig needs the whole file at once, so it is copied into memory first.
        using var memory = new MemoryStream();
        await file.CopyToAsync(memory);
        memory.Position = 0;

        return ReadPdf(memory);
    }

    /// <summary>One block of text per page, so page breaks become chunk boundaries.</summary>
    private static string ReadPdf(Stream pdf)
    {
        using PdfDocument document = PdfDocument.Open(pdf);

        IEnumerable<string> pages = document.GetPages()
            .Select(page => page.Text.Trim())
            .Where(text => text.Length > 0);

        return string.Join(Environment.NewLine + Environment.NewLine, pages);
    }

    private static string Extension(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant();
}
