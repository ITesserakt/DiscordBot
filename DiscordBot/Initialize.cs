using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Teams;

namespace DiscordBot;

public class Initialize {
    private readonly DiscordSocketClient _discord;
    private readonly InteractionService  _interactions;

    public Initialize(DiscordSocketClient? discord = null, InteractionService? interactions = null) {
        _discord      = discord ?? new DiscordSocketClient();
        _interactions = interactions ?? new InteractionService(_discord, new InteractionServiceConfig());

        _discord.Log += logAsync;
    }

    private static Logger Logger => new LoggerConfiguration()
                                    .Enrich.FromLogContext()
                                    .WriteTo.Console()
                                    .MinimumLevel.Debug()
                                    .CreateLogger();

    private static async Task logAsync(LogMessage message) {
        var severity = message.Severity switch {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error    => LogEventLevel.Error,
            LogSeverity.Warning  => LogEventLevel.Warning,
            LogSeverity.Info     => LogEventLevel.Information,
            LogSeverity.Verbose  => LogEventLevel.Verbose,
            LogSeverity.Debug    => LogEventLevel.Debug,
            _                    => LogEventLevel.Information
        };
        Logger.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
        await Task.CompletedTask;
    }

    public IServiceProvider buildServiceProvider() =>
        new ServiceCollection()
            .AddSingleton<IDiscordClient>(_discord)
            .AddSingleton(_interactions)
            .AddSingleton<ITeamRepository>(new MemoryTeamRepository())
            .AddLogging(builder => builder.AddSerilog(Logger))
            .AddSingleton(svc => new CommandService(svc, _discord, _interactions, Logger))
            .BuildServiceProvider();

    public static void loadCommandService(IServiceProvider svc) {
        svc.GetRequiredService<CommandService>();
    }
}