using LanguageExt;

namespace Teams;

public interface ITeamRepository {
    public Task                            addTeam(Team team);
    public Task<IReadOnlyCollection<Team>> fetchTeams();
    public Task<Option<Team>>              getTeam(string name);
}

public class MemoryTeamRepository : ITeamRepository {
    private readonly System.Collections.Generic.HashSet<Team> _teams = new();

    public Task addTeam(Team team) {
        _teams.Find(t => t.Name == team.Name)
              .Some(_ => throw new Exception($"Team with name `{team.Name}` already exists"))
              .None(() => _teams.Add(team));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Team>> fetchTeams() =>
        Task.FromResult<IReadOnlyCollection<Team>>(_teams);

    public Task<Option<Team>> getTeam(string name) =>
        Task.FromResult(_teams.Find(team => team.Name == name));
}