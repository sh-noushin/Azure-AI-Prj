# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file first: restore is then cached unless packages change.
COPY Api.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# ---------- run ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# The Speech SDK needs these native libraries on Linux.
# libasound2 was renamed libasound2t64 in newer Debian, so try both.
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates libssl3 && (apt-get install -y --no-install-recommends libasound2t64 || apt-get install -y --no-install-recommends libasound2) && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]
