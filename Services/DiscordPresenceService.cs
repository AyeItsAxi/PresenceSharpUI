#nullable enable
using System;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;

namespace PresenceSharpUI.Services;

public sealed class DiscordPresenceService : IDisposable
{
    private DiscordRpcClient? _client;
    private RichPresence? _pendingPresence;
    private string? _currentClientId;

    public User? CurrentUser => _client?.CurrentUser;

    public event Action? PresenceUpdated;
    public event Action? ConnectionFailed;

    public void Initialize(string clientId)
    {
        if (_client is not null && _currentClientId == clientId)
            return;

        DisposeClient();

        _client = CreateClient(clientId);
        _currentClientId = clientId;

        _client.Initialize();
    }

    public void SetPresence(RichPresence presence)
    {
        if (_client is null)
            return;

        if (!_client.IsInitialized)
        {
            _pendingPresence = presence;
            return;
        }

        _client.ClearPresence();
        _client.SetPresence(presence);
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

        client.OnPresenceUpdate += HandlePresenceUpdate;
        client.OnConnectionFailed += HandleConnectionFailed;
        client.OnReady += HandleReady;

        return client;
    }

    private void HandlePresenceUpdate(object sender, PresenceMessage e)
    {
        Console.WriteLine("Received Update! {0}", e.Presence);
        PresenceUpdated?.Invoke();
    }

    private void HandleConnectionFailed(object sender, ConnectionFailedMessage e)
    {
        Console.WriteLine("Received Error! {0}", e.FailedPipe);
        ConnectionFailed?.Invoke();
    }
    
    private void HandleReady(object sender, ReadyMessage e)
    {
        if (_client is null || _pendingPresence is null)
            return;

        _client.ClearPresence();
        _client.SetPresence(_pendingPresence);
        _pendingPresence = null;
    }

    private void DisposeClient()
    {
        if (_client is null)
            return;

        _client.OnPresenceUpdate -= HandlePresenceUpdate;
        _client.OnConnectionFailed -= HandleConnectionFailed;
        _client.OnReady -= HandleReady;

        if (_client.IsInitialized)
        {
            _client.ClearPresence();
            _client.Deinitialize();
        }

        _client.Dispose();
        _client = null;
        _currentClientId = null;
    }

    public void Dispose()
    {
        DisposeClient();
    }
}