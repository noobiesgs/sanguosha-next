using System.Collections.Concurrent;
using Injectio.Attributes;
using Microsoft.IO;
using NetFabric.Hyperlinq;

namespace Noobie.SanGuoSha.Network;

[RegisterSingleton, AutoConstructor]
public partial class SendingThread : IDisposable
{
    private readonly SanGuoShaTcpServer _server;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    private readonly SemaphoreSlim _semaphore = new(0, 1);
    private readonly ConcurrentQueue<SendingPacket> _sendingPackets = new(new BlockingCollection<SendingPacket>());
    private readonly CancellationTokenSource _cts = new();

    public void Start()
    {
        Task.Factory.StartNew(SendLoop, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void Send(SanGuoShaTcpClient client, GameDataPacket packet)
    {
        _sendingPackets.Enqueue(new SendingPacket(client, packet));
        _semaphore.Release();
    }

    private async Task SendLoop()
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

            var groups = _sendingPackets.AsValueEnumerable().GroupBy(p => p.Client);
            foreach (var group in groups)
            {
                var client = group.Key;
                if (!client.Online) continue;

                await using var memory = (RecyclableMemoryStream)_memoryStreamManager.GetStream();
                StreamingSerializer.Serialize(memory, group.Count(), group);

                try
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    client.Send(memory.GetReadOnlySequence());
                }
                catch (ObjectDisposedException)
                {

                }
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _semaphore.Dispose();
        _cts.Dispose();
        _sendingPackets.Clear();
    }
}

internal struct SendingPacket
{
    public SendingPacket(SanGuoShaTcpClient client, GameDataPacket packet)
    {
        Client = client;
        Packet = packet;
    }

    public SanGuoShaTcpClient Client;
    public GameDataPacket Packet;
}