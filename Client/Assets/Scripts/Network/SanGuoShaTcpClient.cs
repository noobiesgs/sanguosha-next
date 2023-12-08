using System.IO;
using TouchSocketSlim.Core;
using TouchSocketSlim.Sockets;

namespace Noobie.SanGuoSha.Network
{
    public delegate void ReceivePacketEventHandler(IGameDataPacket packet);

    public delegate void DisconnectedEventHandler();

    public class SanGuoShaTcpClient : TcpClient
    {
        public SanGuoShaTcpClient(IpHost remoteIpHost) : base(remoteIpHost, () => new FixedHeaderPackageAdapter())
        {

        }

        protected override void ReceivedData(byte[] buffer, int offset, int length)
        {
            using var memory = new MemoryStream(buffer, offset, length, false);
            var packets = StreamingSerializer.Deserialize<IGameDataPacket>(memory);
            foreach (var packet in packets)
            {
                OnReceivePacket(packet);
            }
        }

        protected override void OnDisconnected(DisconnectEventArgs e)
        {
            OnDisconnected();
        }

        private void OnReceivePacket(IGameDataPacket packet)
        {
            ReceivePacket?.Invoke(packet);
        }

        private void OnDisconnected()
        {
            Disconnected?.Invoke();
        }

        public event ReceivePacketEventHandler ReceivePacket;
        public new event DisconnectedEventHandler Disconnected;
    }
}
