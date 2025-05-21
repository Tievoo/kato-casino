public enum PlayerStatus
{
    Betting,
    BetsPlaced,
    Deciding,
    Bust,
    Stand,
    Blackjack,
    Waiting
}

public class Player(string username, string connectionId)
{
    // public required string UserId { get; set; }
    // public required string SeatId { get; set; }
    // public required string RoomId { get; set; }
    public string Username { get; set; } = username;
    public string ConnectionId { get; set; } = connectionId;
    public int Bet { get; set; } = 0;
    public List<Card> Hand { get; set; } = [];
    public PlayerStatus Status { get; set; } = PlayerStatus.Waiting;
    public bool IsDealer { get; set; } = false;
    public bool IsPlaying { get; set; } = false;


    public int CalculateBestScore()
    {
        var allSums = new List<int> { 0 };

        foreach (var card in Hand)
        {
            var newSums = new List<int>();
            foreach (var val in card.GetNumericValues())
            {
                foreach (var sum in allSums)
                    newSums.Add(sum + val);
            }
            allSums = newSums;
        }

        return allSums
            .Where(s => s <= 21)
            .DefaultIfEmpty(allSums.Min())
            .Max();
    }

    public int GetPayout(Player dealer)
    {
        var dealerScore = dealer.CalculateBestScore();
        var playerScore = CalculateBestScore();

        if (Status == PlayerStatus.Blackjack) return Bet + Bet * (3/2); // Player has blackjack
        if (playerScore > 21 || Status == PlayerStatus.Bust || playerScore < dealerScore) return 0; // Player busts
        if (dealerScore > 21 || dealer.Status == PlayerStatus.Bust || playerScore > dealerScore) return Bet*2; // Dealer busts
        if (playerScore == dealerScore) return Bet; // Push

        return 0; // Default case, should not happen
    }
}
