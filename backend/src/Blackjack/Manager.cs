public class Manager
{
    private readonly Dictionary<string, Room> _rooms = [];
    private readonly IGameEvents _gameEvents;

    public Manager(IGameEvents gameEvents)
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

    public List<Room> GetAllRooms() => [.. _rooms.Values];
}

