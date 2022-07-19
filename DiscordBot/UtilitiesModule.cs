using Discord;
using Discord.Interactions;

namespace DiscordBot;

public class UtilitiesModule : InteractionModuleBase<SocketInteractionContext> {
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    [SlashCommand("remove_roles_with_prefix", "Removes all roles with provided prefix")]
    public async Task removeRolesWithPrefix(string prefix) {
        var cnt = 0;

        foreach (var role in Context.Guild.Roles) {
            if (!role.Name.StartsWith(prefix)) continue;
            await role.DeleteAsync();
            cnt++;
        }

        await RespondAsync($"Deleted {cnt} roles", ephemeral: true);
    }
}