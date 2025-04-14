public class Manager
{
    private readonly Dictionary<string, Room> _rooms = new();

    public Room GetOrCreateRoom(string roomId, IGameEvents events, int decks = 6)
    {
        if (!_rooms.ContainsKey(roomId))
            _rooms[roomId] = new Room(events, decks, roomId);

        return _rooms[roomId];
    }

    public Room? GetRoom(string roomId)
    {
        return _rooms.TryGetValue(roomId, out var r) ? r : null;
    }

    public List<Room> GetAllRooms() => [.. _rooms.Values];
}
