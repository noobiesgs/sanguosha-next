using System.Collections.Concurrent;
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace Noobie.SanGuoSha.Network;

[RegisterSingleton, AutoConstructor]
public partial class SendingThread : IDisposable
{
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;
    private readonly ILogger<SendingThread> _logger;

    private readonly SemaphoreSlim _semaphore = new(0, int.MaxValue);
    private readonly ConcurrentQueue<SendingPacket> _sendingPackets = new(new BlockingCollection<SendingPacket>());
    private readonly CancellationTokenSource _cts = new();
    private bool _started;

    public void Start()
    {
        if (_started)
        {
            throw new InvalidOperationException("Sending thread already started.");
        }
        _started = true;
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

            if (_sendingPackets.IsEmpty)
            {
                continue;
            }

            var dic = new Dictionary<SanGuoShaTcpClient, List<GameDataPacket>>();
            while (_sendingPackets.TryDequeue(out var pair))
            {
                var (client, packet) = pair;
                if (dic.TryGetValue(client, out var list))
                {
                    list.Add(packet);
                }
                else
                {
                    list = new() { packet };
                    dic[client] = list;
                }
            }

            foreach (var (client, packets) in dic)
            {
                if (!client.Online) continue;

                await using var memory = (RecyclableMemoryStream)_memoryStreamManager.GetStream();
                try
                {
                    StreamingSerializer.Serialize(memory, packets.Count, packets);
                    // ReSharper disable once MethodHasAsyncOverload
                    client.Send(memory.GetReadOnlySequence());
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to send message, client id: {id}, ip: {ip}", client.Id, client.Ip);
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

    public void Deconstruct(out SanGuoShaTcpClient client, out GameDataPacket packet)
    {
        client = Client;
        packet = Packet;
    }

    public SanGuoShaTcpClient Client;
    public GameDataPacket Packet;
}