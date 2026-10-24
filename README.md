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

## Running it with Docker (nothing else to install)

The quickest way to try the app. You need **only Docker** &mdash; the images bring
.NET and Node with them.

```bash
git clone https://github.com/sh-noushin/Azure-AI-Prj.git
cd Azure-AI-Prj

cp .env.example .env     # then fill in your Azure values
docker compose up
```

Open <http://localhost:4300>. Stop it with `Ctrl+C`, or `docker compose down`.

`.env` needs an **API key** from the Azure portal (your AI Services resource,
*Keys and Endpoint*) together with the two endpoints and your deployment names.
A key is used here rather than `az login`, because a container has no login
session of its own. `.env` is git-ignored, so the key stays on your machine.

If port 4300 is taken, set `WEB_PORT=4400` in `.env`.

## Running it without Docker

Useful while developing, because both parts reload as you edit.



- .NET 10 SDK
- Node 20 or newer
- An Azure AI Services resource with two model deployments:
  `gpt-5-mini` and `text-embedding-3-small`
- The **Azure AI User** role on that resource

## Running it

Sign in once. With no API key configured the app uses this session instead,
so nothing secret is stored.

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
npx ng serve        # http://localhost:4300
```

Open http://localhost:4300.

## How the two ways authenticate

| | Running locally | Running in Docker |
|---|---|---|
| Identity | your `az login` session | the API key from `.env` |
| Code | `DefaultAzureCredential` | `AzureKeyCredential` |
| Chosen by | `AzureAi:ApiKey` being empty | `AzureAi:ApiKey` being set |

Every service checks the same setting, so the switch happens in one place per
service and no code changes between the two.

## Ports

Change any of these if something else on your machine already uses them.

The website answers on **<http://localhost:4300>** either way, so the URL is the
same whether you run it with Docker or with the dev server.

| What | Port | Where to change it |
|---|---|---|
| Website, Docker | 4300 | `WEB_PORT` in `.env` |
| Website, dev server | 4300 | `web/angular.json`, under `serve.options.port` &mdash; or `npx ng serve --port 4400` for one run |
| API, dev | 5080 | `api/Properties/launchSettings.json` **and** `web/proxy.conf.json`; both must match |
| API, Docker | not published | nginx reaches it inside the Docker network, so nothing to change |

Running the dev server and Docker at the same time means giving one of them a
different port, since they would otherwise both want 4300.

The API port is not exposed by Docker at all: only the website is reachable
from outside, and it forwards `/api` to the API container internally.

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

## Rules and limits

There is no business domain here: every page takes input, calls Azure and
shows the answer. What the app does have are limits and a few deliberate
behaviours, all enforced in the endpoint files.

| Feature | Rule |
|---|---|
| Chat | The whole conversation is sent with every question, because the model keeps no memory between calls |
| Ähnlichkeit | Both texts are required; the score is cosine similarity between two 1536-number vectors |
| Eigenes Dokument | Upload at most 200 KB of text; it is cut into pieces of about 700 characters, and the 3 closest to the question are used |
| Eigenes Dokument | The model is told to answer **only** from those pieces and to reply "Das steht nicht im Dokument." otherwise, rather than guessing |
| Textanalyse | At most 5000 characters per call, which is the Language service limit |
| Bildanalyse | JPEG, PNG, WebP or GIF, at most 4 MB; tags below 0.7 confidence are dropped as noise |
| Bildanalyse | The description comes from `gpt-5-mini`, not from Vision, because captions are unavailable in Sweden Central |
| Sprache | At most 1000 characters to read aloud; uploads must be WAV and at most 10 MB |
| Übersetzen | At most 5000 characters; the source language is detected, only the target is chosen |
| Sprachassistent | At most 10 MB of audio; the model is told to answer in three short sentences, since the reply is read aloud |
| Sprachassistent | Silence returns a friendly prompt rather than an error |

Two things are deliberately **not** handled, because this is a learning project
and not a product: the uploaded document lives in memory and is lost when the
API restarts, and there is no sign-in, so the app must not be put on a public
URL without one.

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
