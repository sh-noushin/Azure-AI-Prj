namespace AiApp.Models;

/// <summary>Something the Vision service recognised in the picture.</summary>
public record VisionTag(string Name, double Confidence);
