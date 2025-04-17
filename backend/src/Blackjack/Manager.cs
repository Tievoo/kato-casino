public class Manager
{
    private readonly Dictionary<string, Room> _rooms = [];
    private readonly GameEvents _gameEvents;
    private readonly Dictionary<string, string> _players = []; // Dictionary<connectionId, playerId>
    private readonly Dictionary<string, string> _players_conn = []; // Dictionary<playerId, connectionId>

    public Manager(GameEvents gameEvents)
    {
        _gameEvents = gameEvents;
    }

    public Room GetOrCreateRoom(string roomId, int decks = 6)
    {
        if (!_rooms.ContainsKey(roomId))
            _rooms[roomId] = new Room(_gameEvents, decks, roomId);

        return _rooms[roomId];
    }

    public Room? GetRoom(string roomId)
    {
        return _rooms.TryGetValue(roomId, out var r) ? r : null;
    }

    public void AddPlayer(string playerId, string connId)
    {
        if (_players_conn.ContainsKey(playerId))
        {
            string oldConnId = _players_conn[playerId];
            _players.Remove(oldConnId);
        }
        _players_conn[playerId] = connId;
        _players[connId] = playerId;
    }

    public string? GetConnId(string playerId)
    {
        return _players_conn.TryGetValue(playerId, out var connId) ? connId : null;
    }

    public string? GetPlayerId(string connId)
    {
        return _players.TryGetValue(connId, out var playerId) ? playerId : null;
    }


    public List<Room> GetAllRooms() => [.. _rooms.Values];
}

