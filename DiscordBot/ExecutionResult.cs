using Discord.Interactions;
using LanguageExt;
using Teams;

namespace DiscordBot;

public class ExecutionResult : RuntimeResult {
    private ExecutionResult(InteractionCommandError? error, string reason) : base(error, reason) { }
    private ExecutionResult() : base(null, "") { }

    public static ExecutionResult success()                                             => new();
    public static ExecutionResult failure(InteractionCommandError error, string reason) => new(error, reason);

    public static explicit operator ExecutionResult(DomainError error) =>
        new(InteractionCommandError.Unsuccessful, error.Description);
}

public static class ExecutionResultExt {
    public static ExecutionResult toExecutionResult(this DomainError err) => (ExecutionResult)err;

    public static EitherAsync<ExecutionResult, T> toExecution<T>(this EitherAsync<DomainError, T> value) =>
        value.MapLeft(toExecutionResult);

    public static async Task<ExecutionResult> toExecutionTask<T>(this EitherAsync<DomainError, T> value) =>
        await value
              .Right(_ => ExecutionResult.success())
              .Left(x => x.toExecutionResult());

    public static async Task<ExecutionResult> toExecutionTask<T>(this EitherAsync<ExecutionResult, T> value) =>
        await value.Swap().IfLeft(ExecutionResult.success());
}