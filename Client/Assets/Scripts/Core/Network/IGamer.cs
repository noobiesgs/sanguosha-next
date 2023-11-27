using Cysharp.Threading.Tasks;

namespace Noobie.SanGuoSha.Network
{
    public interface IGamer
    {
        UniTask<GameDataPacket> ReceiveAsync();

        void SendAsync(GameDataPacket packet);
    }
}
