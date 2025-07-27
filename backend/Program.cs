using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Registrás SignalR with JSON options
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure JSON serialization for API endpoints
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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
    var hubContext = sp.GetRequiredService<IHubContext<BlackjackHub>>();
    return new GameEvents(hubContext);
});

builder.Services.AddSingleton<BlackjackManager>();

var app = builder.Build();


app.UseCors();

app.MapGet("/", () => "Hello World");

app.MapPost("/blackjack/rooms", (BlackjackManager manager, HttpContext context) =>
{
    string roomId = manager.CreateRoom();
    if (roomId == null)
    {
        context.Response.StatusCode = 500;
        return Results.Problem("Error creating room");
    }
    return Results.Ok(new { roomId });
});

app.MapGet("/blackjack/rooms", (BlackjackManager manager) =>
{
    IEnumerable<object> rooms = manager.GetRooms();
    return Results.Ok(rooms);
});

app.MapHub<BlackjackHub>("/blackjack");

app.Run();
