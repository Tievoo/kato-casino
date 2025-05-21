using System.Text.Json;

public abstract class CommandManager : IManager
{
    private readonly Dictionary<string, (Type payloadType, Func<string, object, Task>)> _handlers = new();

    protected void Register<TPayload>(string commandName, Func<string, TPayload, Task> handler)
    {
        _handlers[commandName] = (
            typeof(TPayload),
            async (connId, raw) =>
            {
                var json = JsonSerializer.Serialize(raw);
                var typedPayload = JsonSerializer.Deserialize<TPayload>(json);
                await handler(connId, typedPayload!);
            }
        );
    }

    public async Task Handle(string command, string connectionId, object payload)
    {
        if (_handlers.TryGetValue(command, out var handlerInfo))
        {
            await handlerInfo.Item2(connectionId, payload);
        }
        else
        {
            Console.WriteLine($"Comando no manejado: {command}");
        }
    }

    public bool CanHandle(string command) => _handlers.ContainsKey(command);
}
