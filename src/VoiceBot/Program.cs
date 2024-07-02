using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TgBotFramework.Core;
using VoiceBot;
using VoiceBot.Services;

await new HostBuilder()
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("settings.json", true, true);
        config.AddJsonFile("logger.json", true, true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var settings = context.Configuration.Get<Settings>();
        if (settings == null)
            throw new ArgumentNullException(nameof(settings), "Не удалось получить настройки приложения");

        services.AddSingleton<IVoiceRecognizer, CompositeRecognizer>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            services.AddTransient<IAudioExtractor, AudioExtractor>();
        else
            services.AddTransient<IAudioExtractor, FfmpegAudioExtractor>();

        services.InitializeBot(settings.TgToken, builder => { builder.WithStates(Assembly.GetExecutingAssembly()); });
        services.AddHostedService<TelegramService>();
    })
    .UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration))
    .ConfigureLogging((host, config) =>
    {
        if (!host.Configuration.GetChildren().Any(s => s.Key.StartsWith("Serilog")))
            config.AddConsole();
    })
    .Build().RunAsync();