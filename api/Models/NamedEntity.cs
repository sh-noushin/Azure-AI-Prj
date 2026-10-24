namespace AiApp.Models;

/// <summary>A thing the service recognised in the text, for example a date or a company.</summary>
public record NamedEntity(string Text, string Category);
