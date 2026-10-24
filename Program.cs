using AiApp.Configuration;
using AiApp.Services;

AzureAiSettings settings = SettingsLoader.Load();
ChatService chat = new(settings);

string question = "Antworte auf Deutsch: Was ist eine digitale Signatur? Zwei Sätze.";

Console.WriteLine($"Frage: {question}");
Console.WriteLine();
Console.WriteLine("Antwort:");
Console.WriteLine(await chat.AskAsync(question));
