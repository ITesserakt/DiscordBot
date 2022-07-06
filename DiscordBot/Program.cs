using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot;
using Serilog;
using Serilog.Events;

static LogEventLevel Convert(LogSeverity severity) => severity switch {
    LogSeverity.Critical => LogEventLevel.Fatal,
    LogSeverity.Error    => LogEventLevel.Error,
    LogSeverity.Warning  => LogEventLevel.Warning,
    LogSeverity.Info     => LogEventLevel.Information,
    LogSeverity.Verbose  => LogEventLevel.Verbose,
    LogSeverity.Debug    => LogEventLevel.Debug,
    _                    => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
};

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .WriteTo.Console()
             .CreateLogger();
var token        = Environment.GetEnvironmentVariable("BOT_TOKEN");
var discord      = new DiscordSocketClient();
var interactions = new InteractionService(discord);
await interactions.AddModuleAsync<EchoModule>(null);

discord.Log   += async message => Log.Write(Convert(message.Severity), message.ToString());
discord.Ready += async () => await interactions.RegisterCommandsGloballyAsync();
discord.InteractionCreated += interaction =>
    interactions.ExecuteCommandAsync(new SocketInteractionContext(discord, interaction), null);

await discord.LoginAsync(TokenType.Bot, token);
await discord.StartAsync();
await Task.Delay(-1);