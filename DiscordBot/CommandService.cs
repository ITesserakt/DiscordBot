using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordBot;

public class CommandService {
    private readonly DiscordSocketClient _client;
    private readonly InteractionService  _interactions;

    private readonly IServiceProvider _services;

    public CommandService(IServiceProvider services, DiscordSocketClient client, InteractionService interactions) {
        _services     = services;
        _client       = client;
        _interactions = interactions;
    }

    public async Task initializeAsync() {
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.SlashCommandExecuted += handleCommandExecution;
    }

    private async Task handleCommandExecution(SocketSlashCommand arg) {
        await _interactions.ExecuteCommandAsync(new SocketInteractionContext(_client, arg), _services);
    }
}