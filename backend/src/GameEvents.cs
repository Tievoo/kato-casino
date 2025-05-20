using Microsoft.AspNetCore.SignalR;

public class GameEvents(IHubContext<GameHub> hubContext)
{
    private readonly IHubContext<GameHub> _hubContext = hubContext;

    public Task SendToRoom(string roomId, string method, object? data)
    {
        return _hubContext.Clients.Group(roomId).SendAsync(method, data);
    }

    public Task SendToRoomExcept(string roomId, string connectionId, string method, object? data)
    {
        return _hubContext.Clients.GroupExcept(roomId, connectionId).SendAsync(method, data);
    }

    public Task SendToPlayer(string connectionId, string method, object? data)
    {
        return _hubContext.Clients.Client(connectionId).SendAsync(method, data);
    }
}
