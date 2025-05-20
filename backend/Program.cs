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
              .AllowCredentials();
    });
});

builder.Services.AddSingleton(sp =>
{
    var hubContext = sp.GetRequiredService<IHubContext<GameHub>>();
    return new GameEvents(hubContext);
});

builder.Services.AddSingleton<BlackjackManager>();

var app = builder.Build();


app.UseCors();

app.MapGet("/", () => "Hello World");

app.MapGet("/blackjack/rooms", (BlackjackManager manager) =>
{
    string[] rooms = [];
    return Results.Ok(rooms);
});

app.MapHub<GameHub>("/hub");

app.Run();
