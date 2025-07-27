using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoomStatus
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
    public RoomStatus Status { get; private set; } = RoomStatus.WaitingForPlayers;

    public Player Dealer { get; set; } = new Player("Dealer", "Dealer");

    public Player[] Seats { get; } = new Player[6];

    public GameEvents Events { get; set; } = events;

    public Deck Deck { get; set; } = new Deck();

    public int currentPlayerIndex = -1;
    static readonly int MIN_PLAYERS = 1;
    static readonly int MS_DELAY = 500;

    public object RoomState()
    {
        return new
        {
            roomId = Id,
            seats = Seats.Select((s, index) => s != null ? new
            {
                username = s.Username,
                seatIndex = index,
                status = s.Status.ToString(),
                hand = s.Hand.Select(c => c.ToString()).ToList(),
                bet = s.Bet
            } : null).ToList(),
            status = Status.ToString(),
            playerTurn = currentPlayerIndex,
            dealerCards = Dealer.Hand.Select(c => c.ToString()).ToList(),
        };
    }

    public void AddPlayer(Player player, int seatIndex)
    {

        if (Seats[seatIndex] != null)
        {
            Events.SendToPlayer(player.ConnectionId, "seatTaken", null);
            return;
        }
        player.Status = Status == RoomStatus.WaitingForBets ? PlayerStatus.Betting : PlayerStatus.Waiting;
        player.SeatIndex = seatIndex;
        Seats[seatIndex] = player;
        Events.SendToRoom(Id, "playerJoined", new
        {
            username = player.Username,
            seatIndex,
            status = player.Status.ToString(),
            hand = player.Hand.Select(c => c.ToString()).ToList(),
            connectionId = player.ConnectionId,
            playerTurn = currentPlayerIndex
        });

        if (Seats.Count(s => s != null) >= MIN_PLAYERS && Status == RoomStatus.WaitingForPlayers)
        {
            WaitForBets();
        }
    }

    public void WaitForBets()
    {
        ChangeStatus(RoomStatus.WaitingForBets);
        ChangePlayersStatus(PlayerStatus.Betting);
        Task.Delay(30000).ContinueWith(_ =>
        {
            if (Status == RoomStatus.WaitingForBets && BetsPlaced())
            {
                Console.WriteLine("Bets placed, starting game...");
                Deal();
            }
            else
            {
                Console.WriteLine("Not enough bets placed, refunding bets...");
                RefundBets();
            }
        });
    }

    public void PlaceBet(int seatIndex, int amount)
    {
        if (Seats[seatIndex] == null || Seats[seatIndex].Status != PlayerStatus.Betting || amount <= 0)
        {
            Events.SendToPlayer(Seats[seatIndex]?.ConnectionId ?? "", "invalidBet", null);
            return;
        }

        Seats[seatIndex].Bet += amount;
        Seats[seatIndex].Status = PlayerStatus.BetsPlaced;

        Events.SendToRoom(Id, "betPlaced", new
        {
            seatIndex,
            amount = Seats[seatIndex].Bet,
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
        return betCount >= MIN_PLAYERS;
    }

    public void RefundBets()
    {
        foreach (var seat in Seats)
        {
            if (seat != null && seat.Bet > 0)
            {
                // seat.Balance += seat.Bet;
                seat.Bet = 0;
            }
        }

        Events.SendToRoom(Id, "betsRefunded", null);
        if (Seats.Count(s => s != null) >= MIN_PLAYERS)
        {
            WaitForBets();
        }
    }

    public void Deal()
    {
        ChangeStatus(RoomStatus.Dealing);

        // Primera carta
        foreach (var seat in Seats)
        {
            if (seat == null) continue;
            if (seat.Status != PlayerStatus.BetsPlaced || seat.Bet <= 0)
            {
                ChangeOnePlayerStatus(seat.SeatIndex, PlayerStatus.Waiting);
                continue;
            }
            Card card = Deck.Draw();
            seat.Hand.Add(card);
            Events.SendToRoom(Id, "cardDealt", new
            {
                seatIndex = Array.IndexOf(Seats, seat),
                card = card.ToString()
            });
            Task.Delay(MS_DELAY).Wait(); // Simulate delay for dealing cards
        }

        Card dealerCard = Deck.Draw();
        Dealer.Hand.Add(dealerCard);
        Events.SendToRoom(Id, "dealerCardDealt", new
        {
            card = dealerCard.ToString()
        });
        Task.Delay(MS_DELAY).Wait(); // Simulate delay for dealer card

        // Segunda carta
        foreach (var seat in Seats)
        {
            if (seat == null) continue;
            if (seat.Status != PlayerStatus.BetsPlaced || seat.Bet <= 0)
            {
                ChangeOnePlayerStatus(seat.SeatIndex, PlayerStatus.Waiting);
                continue;
            }
            Card card = Deck.Draw();
            seat.Hand.Add(card);
            Events.SendToRoom(Id, "cardDealt", new
            {
                seatIndex = Array.IndexOf(Seats, seat),
                card = card.ToString()
            });
            Task.Delay(MS_DELAY).Wait(); // Simulate delay for dealing cards
        }

        dealerCard = Deck.Draw();
        dealerCard.IsVisible = false;
        Dealer.Hand.Add(dealerCard);
        Events.SendToRoom(Id, "dealerCardDealt", new
        {
            card = dealerCard.ToString()
        });

        // Cambiar el estado a Jugando
        StartPlaying();
    }

    public void ChangeStatus(RoomStatus newStatus)
    {
        Status = newStatus;
        Events.SendToRoom(Id, "roomStatus", Status);
    }

    public void ChangeOnePlayerStatus(int seatIndex, PlayerStatus newStatus)
    {
        if (Seats[seatIndex] == null) return;

        Seats[seatIndex].Status = newStatus;
        Events.SendToRoom(Id, "playerStatus", new
        {
            seatIndex,
            status = newStatus.ToString()
        });
    }

    public void ChangePlayersStatus(PlayerStatus newStatus)
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i] != null)
            {
                Seats[i].Status = newStatus;
                Events.SendToRoom(Id, "playerStatus", new
                {
                    seatIndex = i,
                    status = newStatus.ToString()
                });
            }
        }
    }

    public void StartPlaying()
    {
        ChangeStatus(RoomStatus.Playing);
        // Grab every player with a bet and BetsPlaced status and make them deciding. Else, waiting.
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i] != null)
            {
                if (Seats[i].Status == PlayerStatus.BetsPlaced && Seats[i].Bet > 0)
                {
                    ChangeOnePlayerStatus(i, PlayerStatus.Deciding);
                }
                else
                {
                    ChangeOnePlayerStatus(i, PlayerStatus.Waiting);
                }
            }
        }
        
        currentPlayerIndex = Seats.ToList().FindIndex(s => s != null && s.Status == PlayerStatus.Deciding);
        Events.SendToRoom(Id, "playerTurn", new
        {
            playerTurn = currentPlayerIndex,
        });
    }

    public void PlayerAction(int seatIndex, string action)
    {
        if (Seats[seatIndex] == null || Seats[seatIndex].Status != PlayerStatus.Deciding)
        {
            Events.SendToPlayer(Seats[seatIndex]?.ConnectionId ?? "", "invalidAction", null);
            return;
        }

        // bool shouldContinue = true;

        switch (action.ToLower())
        {
            case "hit":
                Hit(seatIndex);
                break;
            case "stand":
                Stand(seatIndex);
                break;
            case "double":
            // Double(seatIndex);
            // break;
            case "surrender":
            // Surrender(seatIndex);
            // break;
            default:
                Events.SendToPlayer(Seats[seatIndex].ConnectionId, "invalidAction", null);
                break;
        }
    }
    public void Hit(int seatIndex)
    {
        if (Seats[seatIndex] == null || Seats[seatIndex].Status != PlayerStatus.Deciding)
        {
            Events.SendToPlayer(Seats[seatIndex]?.ConnectionId ?? "", "invalidAction", null);
            return;
        }

        Card card = Deck.Draw();
        Seats[seatIndex].Hand.Add(card);
        Events.SendToRoom(Id, "cardDealt", new
        {
            seatIndex,
            card = card.ToString()
        });

        int score = Seats[seatIndex].CalculateBestScore();
        if (score > 21)
        {
            Seats[seatIndex].Status = PlayerStatus.Bust;
            Events.SendToRoom(Id, "playerStatus", new
            {
                seatIndex,
                status = PlayerStatus.Bust
            });
            NextPlayer();
        }
    }

    public void Stand(int seatIndex)
    {
        if (Seats[seatIndex] == null || Seats[seatIndex].Status != PlayerStatus.Deciding)
        {
            Events.SendToPlayer(Seats[seatIndex]?.ConnectionId ?? "", "invalidAction", null);
            return;
        }

        Seats[seatIndex].Status = PlayerStatus.Stand;
        Events.SendToRoom(Id, "playerStatus", new
        {
            seatIndex,
            status = PlayerStatus.Stand
        });
        NextPlayer();
    }

    public void NextPlayer()
    {
        // Find the next player. check if the seat exists and is deciding, if the number goes higher than the length of the seats, dealer turn
        if (Seats.All(s => s == null || s.Status != PlayerStatus.Deciding))
        {
            // All players have finished their turn, dealer turn
            // ChangeStatus(RoomStatus.Dealer);
            DealerTurn();
            Console.WriteLine("All players have finished their turn, dealer turn.");
            return;
        }

        currentPlayerIndex = (currentPlayerIndex + 1) % Seats.Length;
        while (Seats[currentPlayerIndex] == null || Seats[currentPlayerIndex].Status != PlayerStatus.Deciding)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % Seats.Length;
        }

        Events.SendToRoom(Id, "playerTurn", new
        {
            playerTurn = currentPlayerIndex
        });
    }

    public void DealerTurn()
    {
        // If all players are bust, dealer wins
        if (Seats.All(s => s == null || s.Status == PlayerStatus.Bust || s.Status == PlayerStatus.Waiting))
        {
            Console.WriteLine("All players are bust, dealer wins.");
            ChangeStatus(RoomStatus.Results);
            // Reset players
            return;
        }

        ChangeStatus(RoomStatus.Dealer);
        currentPlayerIndex = -1; // Reset player index for dealer turn
        Events.SendToRoom(Id, "playerTurn", new
        {
            playerTurn = currentPlayerIndex
        });

        Dealer.Hand.Last().IsVisible = true; // Show dealer's first card
        Events.SendToRoom(Id, "dealerShowCard", new
        {
            card = Dealer.Hand.Last().ToString()
        });
        Task.Delay(MS_DELAY).Wait();
        // Dealer.Status = PlayerStatus.Deciding;

        while (Dealer.CalculateBestScore() < 17)
        {
            Card card = Deck.Draw();
            Dealer.Hand.Add(card);
            Events.SendToRoom(Id, "dealerCardDealt", new
            {
                card = card.ToString()
            });
            Task.Delay(MS_DELAY).Wait(); // Simulate delay for dealing cards
        }

        Dealer.Status = PlayerStatus.Stand;
        Events.SendToRoom(Id, "dealerStatus", new
        {
            status = Dealer.Status.ToString()
        });

        // CalculateResults();
        Console.WriteLine("Dealer has finished their turn, calculating results...");
        Restart();
    }

    public bool SeatBelongsToPlayer(int seatIndex, string playerId)
    {
        return Seats[seatIndex]?.Username == playerId;
    }

    public void Restart()
    {
        foreach (var seat in Seats)
        {
            if (seat != null)
            {
                seat.Hand.Clear();
                seat.Bet = 0;
                seat.Status = PlayerStatus.Waiting;
            }
        }
        Dealer.Hand.Clear();
        Dealer.Status = PlayerStatus.Waiting;
        Deck = new Deck();
        currentPlayerIndex = -1;
        Status = RoomStatus.WaitingForPlayers;
        Events.SendToRoom(Id, "roomState", RoomState());
        if (Seats.Count(s => s != null) >= MIN_PLAYERS)
        {
            WaitForBets();
        }
    }
}
