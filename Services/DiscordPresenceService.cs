#nullable enable
using System;
using DiscordRPC;
using DiscordRPC.Logging;

namespace PresenceSharpUI.Services;

public sealed class DiscordPresenceService : IDisposable
{
    private DiscordRpcClient? _client;
    private string? _currentClientId;

    public User? CurrentUser => _client?.CurrentUser;

    public event Action? PresenceUpdated;
    public event Action? ConnectionFailed;

    public void Initialize(string clientId)
    {
        if (_client is not null && _currentClientId == clientId)
            return;

        _client?.Dispose();

        _client = CreateClient(clientId);
        _currentClientId = clientId;

        _client.Initialize();
    }

    public void SetPresence(RichPresence presence)
    {
        _client?.SetPresence(presence);
    }

    private DiscordRpcClient CreateClient(string clientId)
    {
        var client = new DiscordRpcClient(clientId)
        {
            Logger = new ConsoleLogger
            {
                Level = LogLevel.Warning
            }
        };

        client.OnPresenceUpdate += (_, e) =>
        {
            Console.WriteLine("Received Update! {0}", e.Presence);
            PresenceUpdated?.Invoke();
        };

        client.OnConnectionFailed += (_, e) =>
        {
            Console.WriteLine("Received Error! {0}", e.FailedPipe);
            ConnectionFailed?.Invoke();
        };

        return client;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _client = null;
        _currentClientId = null;
    }
}