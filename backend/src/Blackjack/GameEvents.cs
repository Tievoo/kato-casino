using Microsoft.AspNetCore.SignalR;

public class GameEvents : IGameEvents
{
    private readonly IHubContext<BlackjackHub> _hubContext;

    public GameEvents(IHubContext<BlackjackHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendToRoom(string roomId, string method, object? data)
    {
        return _hubContext.Clients.Group(roomId).SendAsync(method, data);
    }

    public Task SendToPlayer(string connectionId, string method, object? data)
    {
        return _hubContext.Clients.Client(connectionId).SendAsync(method, data);
    }


    public async Task OnCardDealt(string playerId, string roomId, Card card)
    {
        try
        {
            await _hubContext.Clients.Group(roomId).SendAsync("CardDealt", playerId, card?.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending card to {playerId} in room {roomId}: {card}. {ex.Message}");
        }
    }
}
