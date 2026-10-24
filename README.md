# Azure AI Playground

A small web app that tries out every Azure AI service, one page per feature.
C# Web API on the back, Angular on the front.

Built to learn the Azure AI SDKs, so each page does one thing and the code
stays short.

## What it can do

| Page | Azure service | What you do |
|---|---|---|
| Chat | `gpt-5-mini` | Ask a question, get an answer |
| Ähnlichkeit | `text-embedding-3-small` | Compare two texts by meaning |
| Eigenes Dokument | embeddings + chat | Upload a file, ask questions about it |
| Textanalyse | Language | Mood, language, topics and names in a text |
| Bildanalyse | Vision + `gpt-5-mini` | Describe a picture and read the text in it |
| Sprache | Speech | Read text aloud, or transcribe a WAV file |
| Übersetzen | Translator | Translate into seven languages |
| Sprachassistent | Speech + chat | Speak a question, hear the answer |

## What you need

- .NET 10 SDK
- Node 20 or newer
- An Azure AI Services resource with two model deployments:
  `gpt-5-mini` and `text-embedding-3-small`
- The **Azure AI User** role on that resource

## Running it

Sign in once. Everything authenticates through this session.

```bash
az login
```

Start the API:

```bash
cd api
dotnet run          # http://localhost:5080
```

Start the frontend in a second terminal:

```bash
cd web
npm install
npx ng serve        # http://localhost:4200
```

Open http://localhost:4200.

## Settings

`api/appsettings.json` holds the endpoints, the region and the deployment names:

```jsonc
{
  "AzureAi": {
    "OpenAiEndpoint": "https://<resource>.openai.azure.com",
    "ServicesEndpoint": "https://<resource>.cognitiveservices.azure.com/",
    "Region": "swedencentral",
    "ChatDeployment": "gpt-5-mini",
    "EmbeddingDeployment": "text-embedding-3-small",
    "ApiKey": ""
  }
}
```

**There are no secrets in this repository.** Every service is reached with
`DefaultAzureCredential`, which uses your `az login` session locally and a
managed identity when deployed. The `ApiKey` entry exists only as a fallback;
if you ever need it, put the value in `api/appsettings.Development.json`,
which git ignores.

Two things worth knowing, both found by testing against the live endpoints:

- **Speech and Translator** accept the same token, but only through the
  resource's *custom subdomain*. The regional Speech endpoint answers 401.
- **Vision captions** are not offered in Sweden Central, so the picture
  description comes from `gpt-5-mini`, which reads images as well as text.

## How it is put together

```
api/
  Program.cs                      settings, services, endpoints
  Configuration/                  the settings class
  Services/<Feature>Service.cs    one per Azure service
  Endpoints/<Feature>Endpoints.cs one per feature
  Models/                         request and response records

web/src/app/
  api.service.ts                  every call to the API
  layout/nav/                     the navigation
  shared/                         feature-card, answer-box, error-message, wav
  <feature>/                      one folder per page
```

Adding a feature means four small pieces: a service, an endpoint file, an
Angular page, and one line in `layout/nav/nav.ts`.

The frontend uses standalone components, signals and zoneless change
detection. There are no layers and no domain model: every page takes input,
calls Azure and shows the answer.

## If something goes wrong

| Symptom | Cause |
|---|---|
| `401` or `403` from Azure | The **Azure AI User** role is missing on the resource, or `az login` has expired |
| `DeploymentNotFound` | The deployment names in `appsettings.json` do not match the portal |
| `npm install` fails with `edgesOut` | npm is too old for your Node version: `npm install -g npm@latest` |
| The API will not rebuild | A previous `dotnet run` is still holding the file; stop it first |
| The microphone button does nothing | The browser blocked microphone access; allow it for `localhost` |

## Cost

Everything here is pay per use and the amounts are small, but the two model
deployments do bill per token. A budget alert on the subscription is worth
setting up before experimenting.
