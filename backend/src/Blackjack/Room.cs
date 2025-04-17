using System.Threading.Tasks;

public class Room(GameEvents gameEvents, int decks = 6, string? id = null, string privacy = "public")
{
    public string Id { get; set; } = id ?? Guid.NewGuid().ToString();
    public Dictionary<string, Player> Players { get; set; } = [];
    public Deck Deck { get; set; } = new Deck(decks);
    private Player Dealer { get; set; } = new Player { Id = "dealer" };
    public string Privacy { get; set; } = privacy;
    public string Status { get; set; } = "waiting";
    public string PlayerTurn { get; set; } = string.Empty;

    private readonly GameEvents _gameEvents = gameEvents;

    public void AddPlayer(string playerId)
    {
        if (Players.Count >= 6)
            throw new InvalidOperationException("Room is full.");

        Players[playerId] = new Player { Id = playerId };
    }

    public async Task DealInitialCards()
    {
        foreach (var player in Players.Values)
        {
            Card card = Deck.Draw();
            player.Hand.Add(card);
            await _gameEvents.OnCardDealt(player.Id, Id, card);
            Thread.Sleep(500);
        }
        
        Card dealerCard = Deck.Draw();
        Dealer.Hand.Add(dealerCard); // This one should be visible
        await _gameEvents.OnCardDealt(Dealer.Id, Id, dealerCard);
        Thread.Sleep(500);

        foreach (var player in Players.Values)
        {
            Card card = Deck.Draw();
            player.Hand.Add(card);
            await _gameEvents.OnCardDealt(player.Id, Id, card);
            Thread.Sleep(500);
        }

        Card dealerCard2 = Deck.Draw();
        dealerCard2.IsVisible = false; // This one should be hidden
        Dealer.Hand.Add(dealerCard2);
        await _gameEvents.OnCardDealt(Dealer.Id, Id, dealerCard2);

        PlayerTurn = Players.Keys.FirstOrDefault() ?? string.Empty;
        Status = "playing";
        await _gameEvents.OnPlayerTurn(PlayerTurn, Id, "playing");
    }

    public async Task Hit(string playerId)
    {
        if (Players.TryGetValue(playerId, out var player))
        {
            Card card = Deck.Draw();
            player.Hand.Add(card);
            await _gameEvents.OnCardDealt(player.Id, Id, card);

            if (player.CalculateBestScore() > 21) {
                player.IsBusted = true;
                await NextTurn(playerId);
            }
        }
    }

    public async Task Stand(string playerId)
    {
        if (Players.TryGetValue(playerId, out var player))
        {
            player.IsStanding = true;
            await NextTurn(playerId);
        }
    }

    private async Task NextTurn(string? playerId)
    {
        string? next = Players.Keys.SkipWhile(p => p != playerId).Skip(1).FirstOrDefault() ?? null;
        Console.WriteLine($"Next player: {next}");
        if (next == null)
        {
            // Start dealer's turn
            // Show dealer's hidden card
            Dealer.Hand[1].IsVisible = true;
            await _gameEvents.OnCardDealt(Dealer.Id, Id, Dealer.Hand[1]);
            // Iterate over dealer's hand and draw cards until score is 17 or higher
            return;
        }
        PlayerTurn = next;
        await _gameEvents.OnPlayerTurn(PlayerTurn, Id, "playing");
    }
}
