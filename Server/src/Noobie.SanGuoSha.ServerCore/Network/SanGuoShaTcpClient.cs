using System.Collections.Concurrent;
using Injectio.Attributes;
using Microsoft.IO;
using TouchSocketSlim.Sockets;

namespace Noobie.SanGuoSha.Network;

[AutoConstructor, RegisterTransient]
public partial class SanGuoShaTcpClient : SocketClient
{
    private readonly SendingThread _sendingThread;
    private readonly SemaphoreSlim _semaphore = new(0, int.MaxValue);
    private readonly ConcurrentQueue<GameDataPacket> _packetsReceived = new(new BlockingCollection<GameDataPacket>());
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _semaphore.Dispose();
        }
        base.Dispose(disposing);
    }

    public void Receive(GameDataPacket packet)
    {
        _packetsReceived.Enqueue(packet);
        _semaphore.Release();
    }

    public void SendAsync(GameDataPacket packet)
    {
        _sendingThread.Send(this, packet);
    }

    public void Send(GameDataPacket packet)
    {
        ThrowIfDisposed();
        using var memory = (RecyclableMemoryStream)_memoryStreamManager.GetStream();
        var packets = new List<GameDataPacket> { packet };
        StreamingSerializer.Serialize(memory, packets.Count, packets);
        Send(memory.GetReadOnlySequence());
    }
}