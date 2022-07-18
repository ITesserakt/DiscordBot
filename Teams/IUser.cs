namespace Teams;

public interface IUser {
    ulong  Id      { get; }
    string Mention { get; }
}