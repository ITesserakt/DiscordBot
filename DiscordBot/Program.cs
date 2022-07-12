using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot;

var token        = Environment.GetEnvironmentVariable("BOT_TOKEN");
var discord      = new DiscordSocketClient();
var interactions = new InteractionService(discord);
var services     = new Initialize(discord, interactions).buildServiceProvider();

Initialize.loadCommandService(services);
await discord.LoginAsync(TokenType.Bot, token);
await discord.StartAsync();
await Task.Delay(-1);