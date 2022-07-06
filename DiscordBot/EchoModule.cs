using Discord.Interactions;

namespace DiscordBot;

public class EchoModule : InteractionModuleBase<SocketInteractionContext> {
    [SlashCommand("echo", "responds with passed text")]
    async Task echo(string text) {
        await RespondAsync(text);
    }
}