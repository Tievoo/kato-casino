public class BlackjackManager : CommandManager
{
    private readonly GameEvents _gameEvents;
    private readonly Dictionary<string, Room> _rooms = [];

    public BlackjackManager(GameEvents gameEvents)
    {
        _gameEvents = gameEvents;
        Register<JoinRoomPayload>("joinRoom", JoinRoom);
    }

    public string CreateRoom()
    {
        string roomId = Guid.NewGuid().ToString();
        Room room = new(_gameEvents, roomId);
        _rooms.Add(roomId, room);
        return room.Id;
    }

    public Task JoinRoom(string connectionId, JoinRoomPayload payload)
    {
        string roomId = payload.RoomId;


        if (_rooms.TryGetValue(roomId, out _))
        {
            _gameEvents.AddToGroup(connectionId, roomId); // Esta como espectador, digamos. no hace falta avisarle a nadie.
        }

        return Task.CompletedTask;

    }
}

