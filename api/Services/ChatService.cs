using AiApp.Configuration;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;

namespace AiApp.Services;

/// <summary>Sends a question to the chat model and returns the answer.</summary>
public class ChatService
{
    private readonly ChatClient _chat;

    public ChatService(AzureAiSettings settings)
    {
        // DefaultAzureCredential uses your "az login" session, so there is no API key.
        AzureOpenAIClient azure = new(new Uri(settings.OpenAiEndpoint), new DefaultAzureCredential());
        _chat = azure.GetChatClient(settings.ChatDeployment);
    }

    public async Task<string> AskAsync(string question)
    {
        var result = await _chat.CompleteChatAsync(new UserChatMessage(question));
        return result.Value.Content[0].Text;
    }

    /// <summary>
    /// Asks with an extra instruction that tells the model how to behave,
    /// for example "answer only from this text".
    /// </summary>
    public async Task<string> AskAsync(string instruction, string question)
    {
        var result = await _chat.CompleteChatAsync(
            new SystemChatMessage(instruction),
            new UserChatMessage(question));

        return result.Value.Content[0].Text;
    }
}
