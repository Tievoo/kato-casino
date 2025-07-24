using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class BlackjackHub(BlackjackManager blackjackManager) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var roomId = httpContext?.Request.Query["roomId"].ToString();
        var playerId = httpContext?.Request.Query["playerId"].ToString();
        if (!string.IsNullOrEmpty(roomId) && !string.IsNullOrEmpty(playerId))
        {
            // Use playerId as username for now
            var payload = new JoinRoomPayload(roomId, playerId);
            await blackjackManager.Handle("joinRoom", Context.ConnectionId, payload);
        }
        await base.OnConnectedAsync();
    }

    public Task HandleCommand(string command, object data)
    {
        string action = command;
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"[{DateTime.Now}] [blackjack] Action: {action} by {connectionId}");
        return blackjackManager.Handle(action, connectionId, data);
    }
}