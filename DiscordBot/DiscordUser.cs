using Teams;

namespace DiscordBot;

public record DiscordUser(Discord.IUser User) : IUser {
    public ulong  Id      => User.Id;
    public string Mention => User.Mention;
}