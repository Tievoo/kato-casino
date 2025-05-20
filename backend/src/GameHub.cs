using Microsoft.AspNetCore.SignalR;

public class GameHub(BlackjackManager blackjackManager) : Hub
{
    private readonly BlackjackManager blackjackManager = blackjackManager;
}