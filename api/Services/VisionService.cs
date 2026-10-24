using AiApp.Configuration;
using AiApp.Models;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Azure.Identity;

namespace AiApp.Services;

/// <summary>
/// Looks at a picture. The Vision service reads any text in it and names what it sees;
/// the chat model writes the description, because captions are not offered in Sweden Central.
/// </summary>
public class VisionService(AzureAiSettings settings, ChatService chat)
{
    /// <summary>Tags below this confidence are noise.</summary>
    private const float MinimumConfidence = 0.7f;

    private readonly ImageAnalysisClient _vision = settings.UseApiKey
        ? new(new Uri(settings.ServicesEndpoint), new AzureKeyCredential(settings.ApiKey))
        : new(new Uri(settings.ServicesEndpoint), new DefaultAzureCredential());

    public async Task<VisionResult> AnalyseAsync(BinaryData image, string contentType)
    {
        ImageAnalysisResult analysis = await _vision.AnalyzeAsync(
            image,
            VisualFeatures.Read | VisualFeatures.Tags);

        string description = await chat.DescribeImageAsync(image, contentType);

        return new VisionResult(description, ReadText(analysis), ReadTags(analysis));
    }

    /// <summary>Every line of text the service found in the picture (OCR).</summary>
    private static List<string> ReadText(ImageAnalysisResult analysis) =>
        analysis.Read?.Blocks
            .SelectMany(block => block.Lines)
            .Select(line => line.Text)
            .ToList() ?? [];

    private static List<VisionTag> ReadTags(ImageAnalysisResult analysis) =>
        analysis.Tags?.Values
            .Where(tag => tag.Confidence >= MinimumConfidence)
            .Select(tag => new VisionTag(tag.Name, Math.Round(tag.Confidence, 2)))
            .ToList() ?? [];
}
