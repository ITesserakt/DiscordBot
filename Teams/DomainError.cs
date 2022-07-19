namespace Teams;

public record DomainError(string Description);

public sealed record NotFoundBy<TWhat, TBy>(TBy By) : DomainError(
    $"`{typeof(TWhat).Name}` was not found while searching by {By}");

public sealed record AlreadyExist<TWhat>(TWhat What, string where) : DomainError(
    $"`{What}` already exist in {where}"
);