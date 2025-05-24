public enum RoomState
{
    WaitingForBets,
    Dealer,
    Results,
    WaitingForPlayers,
    Playing,
    Dealing
}



public class Room(GameEvents events, string id)
{
    public string Id { get; } = id;
    public RoomState State { get; private set; } = RoomState.WaitingForPlayers;

    public Player Dealer { get; set; } = new Player("Dealer", "Dealer");

    public Player[] Seats { get; } = new Player[6];

    public GameEvents Events { get; set; } = events;

    public Deck Deck { get; set; } = new Deck();

    public int currentPlayerIndex = 0;

    public void AddPlayer(Player player, int seatIndex)
    {

        if (Seats[seatIndex] != null)
        {
            Events.SendToPlayer(player.ConnectionId, "seatTaken", null);
            return;
        }

        Seats[seatIndex] = player;
        Events.SendToRoom(Id, "playerJoined", new
        {
            username = player.Username,
            seatIndex
        });

        if (Seats.Length >= 2 && State == RoomState.WaitingForPlayers)
        {
            ChangeState(RoomState.WaitingForBets);
            Task.Delay(30000).ContinueWith(_ =>
            {
                if (State == RoomState.WaitingForBets && BetsPlaced())
                {
                    Deal();
                }
            });
        }
    }

    public void WaitForBets()
    {
        ChangeState(RoomState.WaitingForBets);
        Task.Delay(30000).ContinueWith(_ =>
        {
            if (State == RoomState.WaitingForBets && BetsPlaced())
            {
                Deal();
            }
            else
            {
                RefundBets();
            }
        });
    }

    public bool BetsPlaced()
    {
        int betCount = 0;
        foreach (var seat in Seats)
        {
            if (seat != null && seat.Bet > 0)
            {
                betCount++;
            }
        }
        return betCount >= 2;
    }

    public void RefundBets()
    {
        foreach (var seat in Seats)
        {
            if (seat != null && seat.Bet > 0)
            {
                // seat.Balance += seat.Bet;
                seat.Bet = 0;
                Events.SendToPlayer(seat.ConnectionId, "betRefunded", new { });
            }
        }
    }

    public void Deal()
    {
        ChangeState(RoomState.Dealing);

        // Primera carta
        foreach (var seat in Seats)
        {
            if (seat != null)
            {
                Card card = Deck.Draw();
                seat.Hand.Add(card);
                Events.SendToRoom(Id, "cardDealt", new
                {
                    seatIndex = Array.IndexOf(Seats, seat),
                    card
                });
            }
        }

        Card dealerCard = Deck.Draw();
        Dealer.Hand.Add(dealerCard);
        Events.SendToRoom(Id, "dealerCardDealt", new
        {
            card = dealerCard
        });

        // Segunda carta
        foreach (var seat in Seats)
        {
            if (seat != null)
            {
                Card card = Deck.Draw();
                seat.Hand.Add(card);
                Events.SendToRoom(Id, "cardDealt", new
                {
                    seatIndex = Array.IndexOf(Seats, seat),
                    card
                });
            }
        }

        dealerCard = Deck.Draw();
        dealerCard.IsVisible = false;
        Dealer.Hand.Add(dealerCard);
        Events.SendToRoom(Id, "dealerCardDealt", new
        {
            card = "hidden"
        });

        // Cambiar el estado a Jugando
        ChangeState(RoomState.Playing);
    }

    public void ChangeState(RoomState newState)
    {
        State = newState;
        Events.SendToRoom(Id, "roomState", State);
    }

    public void DealerTurn()
    {
        // TBI
    }
    
    public void NextPlayer()
    {
        int previousPlayerIndex = currentPlayerIndex;
        currentPlayerIndex = (currentPlayerIndex + 1) % Seats.Length;
        while (Seats[currentPlayerIndex] == null || Seats[currentPlayerIndex].Bet <= 0)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % Seats.Length;
        }

        if (currentPlayerIndex == previousPlayerIndex)
        {
            // No hay jugadores disponibles
            DealerTurn();
            return;
        }

        Events.SendToRoom(Id, "nextPlayer", new
        {
            seatIndex = currentPlayerIndex
        });
    }
    
}
