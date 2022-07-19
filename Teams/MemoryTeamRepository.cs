using LanguageExt;

namespace Teams;

public class MemoryTeamRepository : ITeamRepository {
    private readonly System.Collections.Generic.HashSet<Team> _teams = new();
    private          HashMap<Captain, Team>                   _teamsByCaptain;
    private          HashMap<string, Team>                    _teamsByName;

    public MemoryTeamRepository() {
        _teamsByName    = associateByName();
        _teamsByCaptain = associateByCaptain();
    }

    public EitherAsync<DomainError, Unit> addTeam(Team team) {
        if (_teamsByCaptain.Find(team.Captain).IsSome)
            return new DomainError($"{team.Captain} already has a team");
        if (_teamsByName.Find(team.Name).IsSome)
            return new AlreadyExist<Team>(team, "teams");

        _teams.Add(team);
        _teamsByName    = associateByName();
        _teamsByCaptain = associateByCaptain();
        return Unit.Default;
    }

    public EitherAsync<DomainError, IReadOnlyCollection<Team>> fetchTeams() => _teams;

    public EitherAsync<DomainError, Team> getTeam(string name) =>
        _teamsByName.Find(name)
                    .ToEitherAsync(new NotFoundBy<Team, string>(name) as DomainError);

    public EitherAsync<DomainError, Team> getTeam(Captain captain) =>
        _teamsByCaptain.Find(captain)
                       .ToEitherAsync(new NotFoundBy<Team, Captain>(captain) as DomainError);

    public EitherAsync<DomainError, Unit> modifyTeam(Team old, Func<Team, Team> func) {
        if (!_teams.Remove(old))
            return new NotFoundBy<Team, Team>(old);
        return addTeam(func(old));
    }

    public EitherAsync<DomainError, Unit> deleteTeam(Team team) {
        if (!_teams.Remove(team))
            return new NotFoundBy<Team, Team>(team);

        _teamsByName    = associateByName();
        _teamsByCaptain = associateByCaptain();
        return Unit.Default;
    }

    private HashMap<string, Team>  associateByName()    => _teams.Map(team => (team.Name, team)).ToHashMap();
    private HashMap<Captain, Team> associateByCaptain() => _teams.Map(team => (team.Captain, team)).ToHashMap();
}