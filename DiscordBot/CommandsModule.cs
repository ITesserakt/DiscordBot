using System.Collections.Immutable;
using Discord;
using Discord.Interactions;
using Teams;
using IUser = Teams.IUser;

namespace DiscordBot;

public class CommandsModule : InteractionModuleBase<SocketInteractionContext> {
    private readonly ITeamRepository _repository;

    public CommandsModule(ITeamRepository repository) => _repository = repository;

    [SlashCommand("create_team", "Создать команду")]
    async Task createTeam(string name) {
        var team = new Team(name, null, null, ImmutableList<IUser>.Empty, ImmutableList<IMember>.Empty);
        await _repository.addTeam(team);
        await RespondAsync($"Created command `{name}`", ephemeral: true);
    }

    [SlashCommand("top", "return all registered teams")]
    async Task top() {
        var teams = await _repository.fetchTeams();
        await RespondAsync(embed: new EmbedBuilder {
            Title = "All teams",
            Fields = teams.Select(team => new EmbedFieldBuilder {
                Name  = team.Name,
                Value = team.Members.Count == 0 ? "no members" : string.Join(", ", team.Members)
            }).ToList()
        }.Build());
    }
}