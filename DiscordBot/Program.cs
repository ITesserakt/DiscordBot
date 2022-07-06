using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot;
using Microsoft.Extensions.DependencyInjection;

var token           = Environment.GetEnvironmentVariable("BOT_TOKEN");
var discord         = new DiscordSocketClient();
var interactions    = new InteractionService(discord);
var serviceProvider = new Initialize(discord, interactions).buildServiceProvider();

discord.Ready += serviceProvider.GetService<CommandService>()!.initializeAsync;
await discord.LoginAsync(TokenType.Bot, token);
await discord.StartAsync();
await Task.Delay(-1);