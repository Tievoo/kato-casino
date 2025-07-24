public class BlackjackManager : CommandManager
{
    private readonly GameEvents _gameEvents;
    private readonly Dictionary<string, Room> _rooms = [];

    public BlackjackManager(GameEvents gameEvents)
    {
        _gameEvents = gameEvents;
        Register<JoinRoomPayload>("joinRoom", JoinRoom);
        Register<JoinTablePayload>("joinTable", JoinTable);
    }

    public string CreateRoom()
    {
        string roomId = Guid.NewGuid().ToString();
        Room room = new(_gameEvents, roomId);
        _rooms.Add(roomId, room);
        return room.Id;
    }

    public IEnumerable<object> GetRooms()
    {
        // Return rooms in object list such as { id: string, players: number, status: string }
        return _rooms.Values.Select(room => new
        {
            id = room.Id,
            players = room.Seats.Length,
            status = room.State.ToString()
        });
    }

    public Task JoinRoom(string connectionId, JoinRoomPayload payload)
    {
        string roomId = payload.RoomId;

        if (_rooms.TryGetValue(roomId, out Room? room))
        {
            _gameEvents.AddToGroup(connectionId, roomId);
            // Build seat info
            var seats = room.Seats.Select(seat => seat == null ? null : new {
                username = seat.Username,
                connectionId = seat.ConnectionId
            }).ToArray();
            _gameEvents.SendToPlayer(connectionId, "Welcome", new {
                roomId = room.Id,
                seats,
                status = room.State.ToString()
            });
        }

        return Task.CompletedTask;
    }

    public Task JoinTable(string connectionId, JoinTablePayload payload)
    {
        string roomId = payload.RoomId;

        if (_rooms.TryGetValue(roomId, out Room? room))
        {
            room.AddPlayer(new Player(connectionId, payload.Username), payload.SeatIndex);
        }

        return Task.CompletedTask;
    }
}

