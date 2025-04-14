public class Room(IGameEvents gameEvents, int decks = 6, string? id = null, string privacy = "public")
{
    public string Id { get; set; } = id ?? Guid.NewGuid().ToString();
    public Dictionary<string, Player> Players { get; set; } = [];
    public Deck Deck { get; set; } = new Deck(decks);
    private Player Dealer { get; set; } = new Player { Id = "dealer" };
    public string Privacy { get; set; } = privacy;
    public string Status { get; set; } = "waiting";
    public string PlayerTurn { get; set; } = string.Empty;

    private readonly IGameEvents _gameEvents = gameEvents;

    public void AddPlayer(string playerId)
    {
        if (Players.Count >= 6)
            throw new InvalidOperationException("Room is full.");

        Players[playerId] = new Player { Id = playerId };
    }

    public void DealInitialCards()
    {
        foreach (var player in Players.Values)
        {
            player.Hand.Add(Deck.Draw());
            // _gameEvents.
        }
        
        Dealer.Hand.Add(Deck.Draw()); // This one should be visible

        foreach (var player in Players.Values)
        {
            player.Hand.Add(Deck.Draw());
        }

        Dealer.Hand.Add(Deck.Draw()); // This one should be hidden
    }

    public void Hit(string playerId)
    {
        if (Players.TryGetValue(playerId, out var player))
        {
            player.Hand.Add(Deck.Draw());
            if (player.CalculateBestScore() > 21) {
                player.IsBusted = true;
            }
        }
    }

    public void Stand(string playerId)
    {
        if (Players.TryGetValue(playerId, out var player))
        {
            player.IsStanding = true;
        }
    }
}
