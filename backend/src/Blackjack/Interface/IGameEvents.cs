public interface IGameEvents
{
    Task SendToRoom(string roomId, string method, object? data);
    Task SendToPlayer(string connectionId, string method, object? data);
    Task OnCardDealt(string playerId, string roomId,  Card card);
}
