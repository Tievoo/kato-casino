using Microsoft.AspNetCore.SignalR;

public class BlackjackHub : Hub
{
    private readonly Manager _manager;

    private Dictionary<string, string> _players = []; // Dictionary<connectionId, playerId> 
    private Dictionary<string, string> _players_conn = []; // Dictionary<playerId, connectionId>

    public BlackjackHub(Manager manager)
    {
        _manager = manager;
    }

    private void AddPlayer(string playerId, string connId)
    {
        if (_players_conn.ContainsKey(playerId))
        {
            string oldConnId = _players_conn[playerId];
            _players.Remove(oldConnId);
        }
        _players_conn[playerId] = connId;
        _players[connId] = playerId;
    }

    public override async Task OnConnectedAsync()
    {
        var ctx = Context.GetHttpContext();
        var playerId = ctx?.Request.Query["playerId"];
        var roomId = ctx?.Request.Query["roomId"];

        if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(roomId)) return;

        var room = _manager.GetOrCreateRoom(roomId!);
        AddPlayer(playerId!, Context.ConnectionId);
        room.AddPlayer(playerId!);
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
        await Clients.Caller.SendAsync("Welcome", new
        {
            message = $"Bienvenido {playerId} a la sala {room.Id}",
            players = room.Players
        });
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

    public void StartGame(string roomId)
    {
        Console.WriteLine($"Starting game in room {roomId}");
        var room = _manager.GetRoom(roomId);
        Console.WriteLine($"Room: {room}");
        if (room == null) return;
        room?.DealInitialCards();
    }
}
