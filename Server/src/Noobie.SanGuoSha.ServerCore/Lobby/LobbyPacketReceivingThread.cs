using System.Collections.Concurrent;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby;

[RegisterSingleton, AutoConstructor]
public partial class LobbyPacketReceivingThread : IDisposable
{
    private readonly MessageHandlerFactory _messageHandlerFactory;
    private readonly SemaphoreSlim _semaphore = new(0, int.MaxValue);
    private readonly ConcurrentQueue<ReceivedLobbyPacket> _packets = new(new BlockingCollection<ReceivedLobbyPacket>());
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<LobbyPacketReceivingThread> _logger;
    private bool _started;

    public void Received(SanGuoShaTcpClient client, LobbyPacket packet)
    {
        _packets.Enqueue(new ReceivedLobbyPacket(client, packet));
        _semaphore.Release();
    }

    public void Start()
    {
        if (_started)
        {
            throw new InvalidOperationException("Receiving thread already started.");
        }
        _started = true;
        Task.Factory.StartNew(ReceiveLoop, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task ReceiveLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await _semaphore.WaitAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (_packets.IsEmpty)
            {
                continue;
            }

            while (_packets.TryDequeue(out var p))
            {
                var handler = _messageHandlerFactory.GetMessageHandler(p.Packet);
                try
                {
                    handler.Handle(p.Client, p.Packet);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to handle message: {@message}", p.Packet);
                }
            }
        }
    }

    public void Dispose()
    {
        _packets.Clear();
        _cts.Cancel();
        _semaphore.Dispose();
        _cts.Dispose();
    }
}

internal struct ReceivedLobbyPacket
{
    public ReceivedLobbyPacket(SanGuoShaTcpClient client, LobbyPacket packet)
    {
        Client = client;
        Packet = packet;
    }

    public SanGuoShaTcpClient Client;
    public LobbyPacket Packet;
}