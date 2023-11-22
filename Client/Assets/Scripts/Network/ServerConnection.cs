using System.IO;
using System.Net;

namespace Noobie.SanGuoSha.Network
{
    public class ServerConnection : TcpClient
    {
        public ServerConnection(IPEndPoint endpoint) : base(endpoint)
        {
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            using var stream = new MemoryStream(buffer, (int)offset, (int)size);

            foreach (var _ in StreamingSerializer.Deserialize<GameDataPacket>(stream))
            {

            }
        }
    }
}
