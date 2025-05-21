public interface IManager
{
    Task Handle(string command, string connectionId, object payload);
    bool CanHandle(string command);
}