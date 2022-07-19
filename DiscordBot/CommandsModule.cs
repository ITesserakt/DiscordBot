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
    async Task<RuntimeResult> createTeam(string name, IGuildUser? captain = null) {
        if (string.IsNullOrWhiteSpace(name))
            return ExecutionResult.failure(InteractionCommandError.BadArgs, "Team name should not be empty or blank");

        var author = captain ?? (IGuildUser)Context.User;
        var team = new Team {
            Name    = name,
            Captain = new Captain(new DiscordUser(author))
        };
        return await _repository
                     .addTeam(team)
                     .MapAsync(async _ => {
                         var commandRole = await Context.Guild.CreateRoleAsync($"Team {name}");
                         return await author.AddRoleAsync(commandRole).ToUnit();
                     }).toExecutionTask();
    }

    [SlashCommand("top", "return all registered teams")]
    async Task top() =>
        await _repository
              .fetchTeams()
              .Right(async teams => await RespondAsync(embed: new EmbedBuilder {
                  Title = "All teams",
                  Fields = teams.Select(team => new EmbedFieldBuilder {
                      Name  = team.Name,
                      Value = team.Members.Count == 0 ? "no members" : string.Join(", ", team.Members)
                  }).ToList()
              }.Build()))
              .Left(err => Task.FromException(new Exception(err.Description)))
              .Unwrap();

    [SlashCommand("restore-all", "restores teams info from roles after restart")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    async Task<RuntimeResult> restoreFromRoles() {
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

        await RespondAsync($"Restored {teamRoles.Count} teams");

        return await teamRoles.SequenceSerial(tuple => {
            var (name, role) = tuple;
            var members = Context.Guild.Users.Where(user => user.Roles.Contains(role)).ToSeq();
            var team = new Team {
                Name    = name,
                Members = members.Map(user => new Member(new DiscordUser(user)) as IMember)
            };
            return _repository.addTeam(team);
        }).toExecutionTask();
    }

    [SlashCommand("edit", "Edits various information about team")]
    [RequireContext(ContextType.Guild)]
    async Task<RuntimeResult> editTeam(
        [Choice("captain", 0)] [Choice("coach", 1)]
        int action,
        IGuildUser? user = null) =>
        await _repository
              .getTeam(new Captain(new DiscordUser(Context.User)))
              .toExecution()
              .Bind(team => ((action, user) switch {
                  (0, not null) => team with { Captain = new Captain(new DiscordUser(user)) },
                  (1, not null) => team with { Coach = new Coach(new DiscordUser(user)) },
                  _ => EitherAsync<ExecutionResult, Team>.Left(ExecutionResult.failure(InteractionCommandError.BadArgs,
                      "Unknown args passed"))
              }).Map(edited => (edited, old: team)))
              .Bind(t => _repository.modifyTeam(t.old, _ => t.edited).toExecution())
              .toExecutionTask();
}