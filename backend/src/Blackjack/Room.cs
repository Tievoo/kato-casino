public enum RoomState
{
    WaitingForBets,
    Dealer,
    Results,
    WaitingForPlayers,
    Dealing
}



public class Room(GameEvents events, string id)
{
    public string Id { get; } = id;
    public RoomState State { get; private set; } = RoomState.WaitingForPlayers;

    public Player[] Seats { get; } = new Player[6];

    public GameEvents Events { get; set; } = events;

    public Deck Deck { get; set; } = new Deck();

    public int currentPlayerIndex = 0;

    public void AddPlayer(Player player)
    {
        if (Seats.Length == 6)
        {
            Events.SendToPlayer(player.ConnectionId, "roomFull", null);
            return;
        }

        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i] == null)
            {
                Seats[i] = player;
                // player.SeatId = i.ToString();
                return;
            }
        }

        if (Seats.Length >= 2)
        {
            State = RoomState.WaitingForBets;
            Events.SendToRoom(Id, "roomState", State);
        }
    }
}
