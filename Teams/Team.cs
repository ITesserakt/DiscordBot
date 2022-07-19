using LanguageExt;

namespace Teams;

public record Team {
    private readonly Captain _captain;
    private readonly Coach?  _coach;

    public string       Name    { get; init; }
    public Seq<IMember> Members { get; init; } = Seq<IMember>.Empty;
    public Seq<IUser>   Invites { get; init; } = Seq<IUser>.Empty;

    public Coach? Coach {
        get => _coach;
        init {
            if (_coach is not null)
                Members = Members.Except(new[] { _coach }).ToSeq();
            _coach = value;
            if (value == null) return;
            Members = Members.Add(value);
        }
    }

    public Captain Captain {
        get => _captain;
        init {
            if (_captain is not null)
                Members = Members.Except(new[] { _captain }).ToSeq();
            Members  = Members.Add(value);
            _captain = value;
        }
    }

    public override string ToString() => $"Team: {Name}";
}