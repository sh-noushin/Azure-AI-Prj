using AiApp.Configuration;
using AiApp.Models;
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
    /// Continues a conversation. The earlier turns are sent along every time, because the
    /// model keeps no memory of its own between calls.
    /// </summary>
    public async Task<string> ContinueAsync(string question, IReadOnlyList<ChatTurn> history)
    {
        List<ChatMessage> messages = [];

        foreach (ChatTurn turn in history)
        {
            messages.Add(turn.Role == "assistant"
                ? new AssistantChatMessage(turn.Text)
                : new UserChatMessage(turn.Text));
        }

        messages.Add(new UserChatMessage(question));

        var result = await _chat.CompleteChatAsync(messages);
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

    /// <summary>
    /// Describes a picture. gpt-5-mini can read images as well as text,
    /// so the picture is sent along with the question.
    /// </summary>
    public async Task<string> DescribeImageAsync(BinaryData image, string contentType)
    {
        var result = await _chat.CompleteChatAsync(
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart(
                    "Beschreibe dieses Bild in zwei bis drei Saetzen auf Deutsch."),
                ChatMessageContentPart.CreateImagePart(image, contentType)));

        return result.Value.Content[0].Text;
    }
}
