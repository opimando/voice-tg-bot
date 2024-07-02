FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
# Install FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/* \
    
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VoiceBot/VoiceBot.csproj", "VoiceBot/"]
RUN dotnet restore "VoiceBot/VoiceBot.csproj"
COPY . .
WORKDIR "/src/VoiceBot"
RUN dotnet build "VoiceBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VoiceBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VoiceBot.dll"]
