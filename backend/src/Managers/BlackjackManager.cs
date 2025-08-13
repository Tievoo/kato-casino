public class BlackjackManager : CommandManager
{
    private readonly GameEvents _gameEvents;
    private readonly Dictionary<string, Room> _rooms = [];
    private readonly Dictionary<string, string> _players = new();

    public BlackjackManager(GameEvents gameEvents)
    {
        _gameEvents = gameEvents;
        Register<JoinRoomPayload>("joinRoom", JoinRoom);
        Register<JoinTablePayload>("joinTable", JoinTable);
        Register<PlaceBetPayload>("placeBet", PlaceBet);
        Register<PlayerActionPayload>("playerAction", PlayerAction);
    }

    public void AddPlayer(string connectionId, string username)
    {
        if (_players.TryGetValue(connectionId, out var existingUsername))
        {
            return;
        }

        _players.Add(connectionId, username);
        _gameEvents.AddToGroup(connectionId, "blackjack");
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
            players = room.Seats.Count(s => s != null),
            status = room.Status.ToString()
        });
    }

    public Task JoinRoom(string connectionId, JoinRoomPayload payload)
    {
        string roomId = payload.roomId;

        AddPlayer(connectionId, payload.username);

        if (_rooms.TryGetValue(roomId, out Room? room))
        {
            _gameEvents.AddToGroup(connectionId, roomId);
            var seats = room.Seats;
            for (int i = 0; i < seats.Length; i++)
            {
                if (seats[i] != null && seats[i].Username == payload.username)
                {
                    seats[i].ConnectionId = connectionId;
                }
            }
            _gameEvents.SendToPlayer(connectionId, "roomState", room.RoomState());
        }

        return Task.CompletedTask;
    }

    public Task JoinTable(string connectionId, JoinTablePayload payload)
    {
        string roomId = payload.roomId;
        _players.TryGetValue(connectionId, out var existingUsername);

        if (existingUsername == null)
        {
            _gameEvents.SendToPlayer(connectionId, "error", "You must join a room first.");
            return Task.CompletedTask;
        }

        if (_rooms.TryGetValue(roomId, out Room? room))
        {
            room.AddPlayer(new Player(existingUsername, connectionId), payload.seatIndex);
        }

        return Task.CompletedTask;
    }

    public Task PlaceBet(string connectionId, PlaceBetPayload payload)
    {
        _players.TryGetValue(connectionId, out var playerId);
        if (playerId == null)
        {
            _gameEvents.SendToPlayer(connectionId, "error", "You must join a room first.");
            return Task.CompletedTask;
        }

        if (_rooms.TryGetValue(payload.roomId, out Room? room))
        {
            if (!room.SeatBelongsToPlayer(payload.seatIndex, playerId))
            {
                _gameEvents.SendToPlayer(connectionId, "error", "You are not seated at this table.");
                return Task.CompletedTask;
            }
            room.PlaceBet(payload.seatIndex, payload.amount);
        }
        else
        {
            _gameEvents.SendToPlayer(connectionId, "error", "Room not found.");
        }

        return Task.CompletedTask;
    }
    
    public Task PlayerAction(string connectionId, PlayerActionPayload payload)
    {
        _players.TryGetValue(connectionId, out var playerId);
        if (playerId == null)
        {
            _gameEvents.SendToPlayer(connectionId, "error", "You must join a room first.");
            return Task.CompletedTask;
        }

        if (_rooms.TryGetValue(payload.roomId, out Room? room))
        {
            if (!room.SeatBelongsToPlayer(payload.seatIndex, playerId))
            {
                _gameEvents.SendToPlayer(connectionId, "error", "You are not seated at this table.");
                return Task.CompletedTask;
            }
            room.PlayerAction(payload.seatIndex, payload.action);
        }
        else
        {
            _gameEvents.SendToPlayer(connectionId, "error", "Room not found.");
        }

        return Task.CompletedTask;
    }
}

