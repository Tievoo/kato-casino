using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Registrás SignalR
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 👈 importante para SignalR
    });
});

builder.Services.AddSingleton<IGameEvents>(sp =>
{
    var hubContext = sp.GetRequiredService<IHubContext<BlackjackHub>>();
    return new GameEvents(hubContext);
});

builder.Services.AddSingleton<Manager>();

var app = builder.Build();


app.UseCors();

app.MapGet("/", () => "Hello World!");

app.MapGet("/blackjack/rooms", (Manager manager) =>
{
    var rooms = manager.GetAllRooms();
    return Results.Ok(rooms);
});

app.MapHub<BlackjackHub>("/blackjack");

app.Run();
