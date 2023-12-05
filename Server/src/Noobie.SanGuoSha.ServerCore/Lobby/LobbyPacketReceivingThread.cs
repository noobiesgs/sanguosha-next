using System.Collections.Concurrent;
using Injectio.Attributes;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby;

[RegisterSingleton, AutoConstructor]
public partial class LobbyPacketReceivingThread : IDisposable
{
    private readonly MessageHandlerFactory _messageHandlerFactory;
    private readonly SemaphoreSlim _semaphore = new(0, 1);
    private readonly ConcurrentQueue<ReceivedLobbyPacket> _packets = new(new BlockingCollection<ReceivedLobbyPacket>());
    private readonly CancellationTokenSource _cts = new();

    public void Received(SanGuoShaTcpClient client, LobbyPacket packet)
    {
        _packets.Enqueue(new ReceivedLobbyPacket(client, packet));
        _semaphore.Release();
    }

    public void Start()
    {
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

            while (_packets.TryDequeue(out var p))
            {
                var handler = _messageHandlerFactory.GetMessageHandler(p.Packet);
                handler.Handle(p.Client, p.Packet);
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