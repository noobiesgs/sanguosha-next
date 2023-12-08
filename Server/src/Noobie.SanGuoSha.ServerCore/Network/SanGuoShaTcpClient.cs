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
    private readonly ConcurrentQueue<IGameDataPacket> _packetsReceived = new(new BlockingCollection<IGameDataPacket>());
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            _semaphore.Dispose();
        }
        base.Dispose(disposing);
    }

    public void Receive(IGameDataPacket packet)
    {
        _packetsReceived.Enqueue(packet);
        _semaphore.Release();
    }

    public void SendAsync(IGameDataPacket packet)
    {
        _sendingThread.Send(this, packet);
    }

    public void Send(IGameDataPacket packet)
    {
        ThrowIfDisposed();
        using var memory = (RecyclableMemoryStream)_memoryStreamManager.GetStream();
        var packets = new List<IGameDataPacket> { packet };
        StreamingSerializer.Serialize(memory, packets.Count, packets);
        Send(memory.GetReadOnlySequence());
    }
}