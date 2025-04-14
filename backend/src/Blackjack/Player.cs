public class Player
{
    public required string Id { get; set; }
    public List<Card> Hand { get; set; } = [];
    public bool IsStanding { get; set; } = false;
    public bool IsBusted { get; set; } = false;

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
}
