public class Card
{
    public required string Suit { get; set; }
    public required string Value { get; set; }

    public int[] GetNumericValues()
    {
        return Value switch
        {
            "A" => [1, 11],
            "J" or "Q" or "K" => new[] { 10 },
            _ => [int.Parse(Value)]
        };
    }

    public override string ToString() => $"{Value}{Suit}";
}
