using Cysharp.Threading.Tasks;

namespace Noobie.SanGuoSha.Network;

public class ServerGamer : IGamer
{
    public UniTask<GameDataPacket> ReceiveAsync()
    {
        throw new NotImplementedException();
    }

    public void SendAsync(GameDataPacket packet)
    {
        throw new NotImplementedException();
    }
}