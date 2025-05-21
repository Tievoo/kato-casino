using Microsoft.AspNetCore.SignalR;

public class GameHub(BlackjackManager blackjackManager) : Hub
{
    // private readonly BlackjackManager blackjackManager = _blackjackManager;

    private readonly Dictionary<string, IManager> _managers = new()
    {
        { "blackjack", blackjackManager }
    };

    public Task HandleCommand(string command, object data)
    {
        string game = command.Split(":")[0];
        string action = command.Split(":")[1];
        string connectionId = Context.ConnectionId;

        if (_managers.TryGetValue(game, out IManager? manager))
        {
            return manager.Handle(action, connectionId, data);
        }
        else
        {
            return Task.CompletedTask;
        }
    }
}