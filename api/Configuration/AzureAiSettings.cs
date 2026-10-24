namespace AiApp.Configuration;

/// <summary>
/// Everything the app needs to reach Azure AI, read from appsettings.json.
/// The real ApiKey lives in appsettings.Development.json, which git ignores.
/// </summary>
public class AzureAiSettings
{
    /// <summary>For chat and embeddings (the OpenAI models).</summary>
    public string OpenAiEndpoint { get; set; } = "";

    /// <summary>For speech, vision, language and translation.</summary>
    public string ServicesEndpoint { get; set; } = "";

    /// <summary>Azure region, needed by the Speech SDK.</summary>
    public string Region { get; set; } = "";

    public string ChatDeployment { get; set; } = "";

    public string EmbeddingDeployment { get; set; } = "";

    /// <summary>
    /// Key for the Speech and Vision SDKs. Leave empty in appsettings.json and put the
    /// real value in appsettings.Development.json, which is never committed.
    /// Chat and embeddings do not need it: they use az login.
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// True when a key was supplied. Running locally you log in with "az login" and leave
    /// this empty; in a container there is no login, so the key is used instead.
    /// </summary>
    public bool UseApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}
