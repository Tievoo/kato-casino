public class Deck
{
    private static readonly string[] Suits = { "♠️", "♥️", "♦️", "♣️" };
    private static readonly string[] Values = 
        { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

    private readonly Stack<Card> Cards;

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

    public Card Draw() => Cards.Pop();

    public void Shuffle()
    {
        var cards = Cards.ToArray();
        Cards.Clear();
        foreach (var card in cards.OrderBy(_ => Guid.NewGuid()))
            Cards.Push(card);
    }

    public int Remaining => Cards.Count;
}
