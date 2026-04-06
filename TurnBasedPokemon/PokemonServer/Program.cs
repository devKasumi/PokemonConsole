using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PokemonServer.Hubs;
using PokemonServer.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add SignalR Services
builder.Services.AddSignalR();

// 2. Add Matchmaking as a Singleton (Only one instance exists on the server)
builder.Services.AddSingleton<MatchmakingService>();

var app = builder.Build();

// 3. Map the BattleHub to an endpoint URL
app.MapHub<BattleHub>("/battlehub");

// 4. A simple HTTP GET endpoint to check if the server is running
app.MapGet("/", () => "Pokemon PvP Server is Online and running SignalR!");

app.Run();