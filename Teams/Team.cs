namespace Teams;

public interface IMember : IUser { }

public record Member(IUser User) : IMember {
    public uint Id => User.Id;
}

public record Captain(IUser User) : IMember {
    public uint Id => User.Id;
}

public record Coach(IUser User) : IMember {
    public uint Id => User.Id;
}

public interface IUser {
    uint Id { get; }
}

public record Team(string                 Name, Captain Captain, Coach Coach, IReadOnlyList<IUser> Invites,
                   IReadOnlyList<IMember> Members);