namespace Teams;

public interface IMember { }

public record Member(IUser User) : IMember {
    public ulong  Id      => User.Id;
    public string Mention => User.Mention;

    public override string ToString() => Mention;
}

public record Captain(IUser User) : IMember {
    public ulong  Id      => User.Id;
    public string Mention => User.Mention;

    public override string ToString() => $"Captain: {Mention}";
}

public record Coach(IUser User) : IMember {
    public ulong  Id      => User.Id;
    public string Mention => User.Mention;

    public override string ToString() => $"Coach: {Mention}";
}