using Microsoft.AspNetCore.SignalR;

public class BlackjackHub : Hub, IGameEvents
{
    private readonly Manager _manager;

    private Dictionary<string, string> _players = []; // Dictionary<connectionId, playerId> 
    private Dictionary<string, string> _players_conn = []; // Dictionary<playerId, connectionId>

    public BlackjackHub(Manager manager)
    {
        _manager = manager;
    }

    private void addPlayer(string playerId, string connId)
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

        var room = _manager.GetOrCreateRoom(roomId!, this);
        addPlayer(playerId!, Context.ConnectionId);
        room.AddPlayer(playerId!);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId!);
        await Clients.Caller.SendAsync("Welcome", $"Bienvenido {playerId} a la sala {roomId}");
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
        var room = _manager.GetOrCreateRoom(roomId, this);
        room.AddPlayer(Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Caller.SendAsync("Welcome", $"Bienvenido {playerId} a la sala {roomId}");
    }

    public async Task StartGame(string roomId)
    {
        var room = _manager.GetRoom(roomId);
        room?.DealInitialCards();
    }

    public async Task OnCardDealt(string playerId, string roomId, Card card)
    {
        await Clients.GroupExcept(roomId, Context.ConnectionId).SendAsync("CardDealt", playerId);
        await Clients.Client(_players[playerId]).SendAsync("CardDealtShow", playerId, card);
    }
}
