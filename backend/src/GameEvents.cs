using Microsoft.AspNetCore.SignalR;

public class GameEvents(IHubContext<BlackjackHub> hubContext)
{
    private readonly IHubContext<BlackjackHub> _hubContext = hubContext;

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

    public Task AddToGroup(string connectionId, string groupName)
    {
        return _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }
}
