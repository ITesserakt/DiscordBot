using System.Collections.Immutable;
using LanguageExt;

namespace Teams;

public interface ITeamRepository {
    public Task                            addTeam(Team team);
    public Task<IReadOnlyCollection<Team>> fetchTeams();
    public Task<Option<Team>>              getTeam(string  name);
    public Task<Option<Team>>              getTeam(Captain captain);

    public Task modifyTeam(Team old, Func<Team, Team> func);
}

public class MemoryTeamRepository : ITeamRepository {
    private readonly LockCookie                               _lock  = new();
    private readonly System.Collections.Generic.HashSet<Team> _teams = new();
    private          ImmutableDictionary<Captain, Team>       _teamsByCaptain;
    private          ImmutableDictionary<string, Team>        _teamsByName;

    public MemoryTeamRepository() {
        _teamsByName    = associateByName();
        _teamsByCaptain = associateByCaptain();
    }

    public Task addTeam(Team team) {
        if (_teamsByName.TryGetValue(team.Name, out _))
            throw new Exception($"Team with name `{team.Name}` already exists");
        if (_teamsByCaptain.TryGetValue(team.Captain, out _))
            throw new Exception($"{team.Captain.Mention} already has a team");

        _teams.Add(team);
        _teamsByName    = associateByName();
        _teamsByCaptain = associateByCaptain();
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Team>> fetchTeams() =>
        Task.FromResult<IReadOnlyCollection<Team>>(_teams);

    public Task<Option<Team>> getTeam(string name) {
        if (_teamsByName.TryGetValue(name, out var team)) return Task.FromResult<Option<Team>>(team);
        return Task.FromResult(Option<Team>.None);
    }

    public Task<Option<Team>> getTeam(Captain captain) {
        if (_teamsByCaptain.TryGetValue(captain, out var team)) return Task.FromResult<Option<Team>>(team);
        return Task.FromResult(Option<Team>.None);
    }

    public Task modifyTeam(Team old, Func<Team, Team> func) {
        if (!_teams.Remove(old))
            throw new Exception($"Cannot find specified team: {old.Name}");
        _teams.Add(func(old));
        return Task.CompletedTask;
    }

    private ImmutableDictionary<string, Team> associateByName() => _teams.ToImmutableDictionary(team => team.Name);

    private ImmutableDictionary<Captain, Team> associateByCaptain() =>
        _teams.ToImmutableDictionary(team => team.Captain);
}