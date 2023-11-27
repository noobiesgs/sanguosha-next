using TouchSocketSlim.Core;
using TouchSocketSlim.Sockets;

namespace Noobie.SanGuoSha.Network
{
    public class SanGuoShaTcpClient : TcpClient
    {
        public SanGuoShaTcpClient(IpHost remoteIpHost) : base(remoteIpHost, () => new FixedHeaderPackageAdapter())
        {
            Connected = OnConnected;
            Disconnected = OnDisconnected;
            Received = OnReceived;
        }

        private void OnReceived(TcpClient _, byte[] buffer, int offset, int length)
        {

        }

        private void OnDisconnected(ITcpClientBase _, DisconnectEventArgs e)
        {

        }

        private void OnConnected(ITcpClient _, ConnectedEventArgs e)
        {

        }
    }
}
