using Microsoft.Extensions.Configuration;

namespace AiApp.Configuration;

/// <summary>Reads appsettings.json into an <see cref="AzureAiSettings"/> object.</summary>
public static class SettingsLoader
{
    public static AzureAiSettings Load()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)  // secrets, git-ignored
            .AddUserSecrets<AzureAiSettings>(optional: true)  // for API keys later, never in a file
            .AddEnvironmentVariables()
            .Build();

        AzureAiSettings settings = config.GetSection("AzureAi").Get<AzureAiSettings>()
            ?? throw new InvalidOperationException("Section 'AzureAi' is missing from appsettings.json.");

        if (string.IsNullOrWhiteSpace(settings.OpenAiEndpoint))
        {
            throw new InvalidOperationException("AzureAi:OpenAiEndpoint is not set in appsettings.json.");
        }

        return settings;
    }
}
