using TouchSocketSlim.Core;
using TouchSocketSlim.Sockets;

namespace Noobie.SanGuoSha.Network
{
    public class SanGuoShaTcpClient : TcpClient
    {
        public SanGuoShaTcpClient(IpHost remoteIpHost) : base(remoteIpHost, () => new FixedHeaderPackageAdapter())
        {
        }
    }
}
