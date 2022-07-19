using Discord.Interactions;
using LanguageExt;
using Teams;

namespace DiscordBot;

public static class DomainErrorExt {
    public static ExecuteResult toExecuteResult(this DomainError err) =>
        ExecuteResult.FromError(InteractionCommandError.Unsuccessful, err.Description);

    public static EitherAsync<ExecuteResult, T> toExecuteResult<T>(this EitherAsync<DomainError, T> value) =>
        value.MapLeft(toExecuteResult);
}