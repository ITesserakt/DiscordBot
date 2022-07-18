using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LanguageExt;
using Teams;

namespace DiscordBot;

[Group("team", "Team management commands")]
public class CommandsModule : InteractionModuleBase<SocketInteractionContext> {
    private readonly ITeamRepository _repository;

    public CommandsModule(ITeamRepository repository) => _repository = repository;

    [SlashCommand("create", "Creates new team")]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    async Task createTeam(string name, IGuildUser? captain = null) {
        if (string.IsNullOrWhiteSpace(name)) {
            await RespondAsync("Team name should not be empty or blank");
            return;
        }

        var author = captain ?? (IGuildUser)Context.User;
        var team = new Team {
            Name    = name,
            Captain = new Captain(new DiscordUser(author))
        };
        await _repository.addTeam(team);
        var commandRole = await Context.Guild.CreateRoleAsync($"Team {name}");
        await author.AddRoleAsync(commandRole);
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

    [SlashCommand("restore-all", "restores teams info from roles after restart")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    async Task restoreFromRoles() {
        var teamRoles =
            Context.Guild.Roles
                   .Select(role => {
                       var pattern = new Regex("Team (.+)");
                       var match   = pattern.Match(role.Name);
                       return !match.Success ? Option<(string, SocketRole)>.None : (match.Groups[1].Value, role);
                   })
                   .Filter(it => it.IsSome)
                   .Select(it => it.IfNoneUnsafe((null, null)!))
                   .ToList();

        foreach (var (name, role) in teamRoles) {
            var members = Context.Guild.Users.Where(user => user.Roles.Contains(role)).ToSeq();
            var team = new Team {
                Name    = name,
                Members = members.Map(user => new Member(new DiscordUser(user)) as IMember)
            };
            await _repository.addTeam(team);
        }

        await RespondAsync($"Restored {teamRoles.Count} teams");
    }

    [SlashCommand("edit", "Edits various information about team")]
    [RequireContext(ContextType.Guild)]
    async Task editTeam([Choice("captain", 0)] [Choice("coach", 1)] int action, IGuildUser? user = null) {
        var author = new Captain(new DiscordUser(Context.User));
        var team = await _repository
                         .getTeam(author)
                         .IfNoneAsync(async () => throw new Exception($"No team associated with {author.Mention}"));

        var editedTeam = (action, user) switch {
            (0, not null) => team with { Captain = new Captain(new DiscordUser(user)) },
            (1, not null) => team with { Coach = new Coach(new DiscordUser(user)) },
            _             => throw new ArgumentException("Unknown argument passed", action.ToString())
        };

        await _repository.modifyTeam(team, _ => editedTeam);
    }
}