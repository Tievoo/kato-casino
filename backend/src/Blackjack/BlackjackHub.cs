using Microsoft.AspNetCore.SignalR;

public class BlackjackHub : Hub
{
    private readonly Manager _manager;

    public BlackjackHub(Manager manager)
    {
        _manager = manager;
    }
    public override async Task OnConnectedAsync()
    {
        var ctx = Context.GetHttpContext();
        var playerId = ctx?.Request.Query["playerId"];
        var roomId = ctx?.Request.Query["roomId"];

        if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(roomId)) return;

        var room = _manager.GetOrCreateRoom(roomId!);
        _manager.AddPlayer(playerId!, Context.ConnectionId);
        room.AddPlayer(playerId!);
        await Clients.Group(room.Id).SendAsync("PlayerConnected", playerId);
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
        await Clients.Caller.SendAsync("Welcome", room);
    }

    public async Task SendToRoom(string roomId, string method, object? data)
    {
        await Clients.Group(roomId).SendAsync(method, data);
    }

    public async Task SendToPlayer(string connectionId, string method, object? data)
    {
        await Clients.Client(connectionId).SendAsync(method, data);
    }

    public async Task JoinRoom(string roomId, string playerId)
    {
        var room = _manager.GetOrCreateRoom(roomId);
        room.AddPlayer(Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Caller.SendAsync("Welcome", $"Bienvenido {playerId} a la sala {roomId}");
    }

    public async Task Hit(string roomId)
    {
        var room = _manager.GetRoom(roomId);
        if (room == null) return;
        await room.Hit(_manager.GetPlayerId(Context.ConnectionId)!);
    }

    public async Task Stand(string roomId)
    {
        var room = _manager.GetRoom(roomId);
        if (room == null) return;
        await room.Stand(_manager.GetPlayerId(Context.ConnectionId)!);
    }

    public void StartGame(string roomId)
    {
        Console.WriteLine($"Starting game in room {roomId}");
        var room = _manager.GetRoom(roomId);
        Console.WriteLine($"Room: {room}");
        if (room == null) return;
        room?.DealInitialCards();
    }
}
