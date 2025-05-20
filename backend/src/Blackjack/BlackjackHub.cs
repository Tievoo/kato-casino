using Microsoft.AspNetCore.SignalR;

public class BlackjackHub : Hub
{
    private readonly Manager _manager;

    public BlackjackHub(Manager manager)
    {
        _manager = manager;
    }
    
}
