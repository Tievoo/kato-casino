public class Deck
{
    private static readonly string[] Suits = { "♠️", "♥️", "♦️", "♣️" };
    private static readonly string[] Values = 
        { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

    private Stack<Card> Cards;
    private readonly List<Card> UsedCards = [];

    public Deck(int numberOfDecks = 1)
    {
        var cards = new List<Card>();

        for (int d = 0; d < numberOfDecks; d++)
        {
            foreach (var suit in Suits)
            {
                foreach (var value in Values)
                {
                    cards.Add(new Card { Suit = suit, Value = value });
                }
            }
        }

        // Shuffle
        Cards = new Stack<Card>(cards.OrderBy(_ => Guid.NewGuid()));
    }

    public Card Draw() {
        var card = Cards.Pop();
        UsedCards.Add(card);
        return card;
    }

    public void Shuffle()
    {
        var shuffledCards = UsedCards.Concat(Cards).OrderBy(_ => Guid.NewGuid());
        Cards.Clear();
        UsedCards.Clear();

        Cards = new Stack<Card>(shuffledCards);
    }

    public int Remaining => Cards.Count;
}
