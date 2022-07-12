using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog.Events;

namespace DiscordBot;

public class CommandService {
    private readonly DiscordSocketClient _client;
    private readonly InteractionService  _interactions;
    private readonly Serilog.ILogger     _logger;

    private readonly IServiceProvider _services;

    public CommandService(IServiceProvider services, DiscordSocketClient client, InteractionService interactions,
                          Serilog.ILogger  logger) {
        _services     = services;
        _client       = client;
        _interactions = interactions;
        _logger       = logger;

        _client.Ready                      += initializeAsync;
        _interactions.SlashCommandExecuted += handleSlashCommandExecuted;
        _client.InteractionCreated         += handleInteractionCreated;
    }

    private async Task initializeAsync() {
        var modules = await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        foreach (var info in modules) {
            _logger.Debug("Loading {Module} module", info.Name);
        }

        var commands = await _interactions.RegisterCommandsToGuildAsync(490951935894093850);
        foreach (var command in commands) {
            _logger.Debug("Registered {Command} slash command into {Guild}", command.Name, command.GuildId);
        }
    }

    private async Task handleInteractionCreated(SocketInteraction socketInteraction) {
        var context = new SocketInteractionContext(_client, socketInteraction);
        await _interactions.ExecuteCommandAsync(context, _services);
    }

    private async Task handleSlashCommandExecuted(SlashCommandInfo info, IInteractionContext ctx, IResult result) {
        var name = $"{info.Module.Name}#{info.Name}";
        (string logMessage, var severity, EmbedBuilder userMessage) = result switch {
            { IsSuccess: true } => (
                $"Successfully executed command `{name}` for {ctx.User}",
                LogEventLevel.Information,
                new EmbedBuilder()
                    .WithDescription("✅")
                    .WithColor(Color.Green)),

            ExecuteResult { Error: InteractionCommandError.Exception } r => (
                $"Command `{name}` fails with exception for {ctx.User}: {r.Exception}",
                LogEventLevel.Error,
                new EmbedBuilder()
                    .WithDescription($"Failed with exception: {r.Exception.Message}. Contact with developers")
                    .WithColor(Color.Red)),

            { IsSuccess: false } => (
                $"Execution of `{name}` ended with error for {ctx.User}: {result.ErrorReason}",
                LogEventLevel.Warning,
                new EmbedBuilder()
                    .WithDescription("❌")
                    .WithColor(Color.Orange))
        };

        _logger.Write(severity, "{Message}", logMessage);
        if (!ctx.Interaction.HasResponded)
            await ctx.Interaction.RespondAsync(embed: userMessage.Build(), ephemeral: true);
    }
}