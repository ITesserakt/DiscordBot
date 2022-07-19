using LanguageExt;

namespace Teams;

public interface ITeamRepository {
    public EitherAsync<DomainError, Unit>                      addTeam(Team team);
    public EitherAsync<DomainError, IReadOnlyCollection<Team>> fetchTeams();
    public EitherAsync<DomainError, Team>                      getTeam(string  name);
    public EitherAsync<DomainError, Team>                      getTeam(Captain captain);
    public EitherAsync<DomainError, Unit>                      modifyTeam(Team old, Func<Team, Team> func);
    public EitherAsync<DomainError, Unit>                      deleteTeam(Team team);

    public EitherAsync<DomainError, Unit> modifyTeam(string name, Func<Team, Team> func) =>
        getTeam(name).Bind(old => modifyTeam(old, func));
}