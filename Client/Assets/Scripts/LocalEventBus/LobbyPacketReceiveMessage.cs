using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.LocalEventBus
{
    public struct LobbyPacketReceiveMessage
    {
        public LobbyPacketReceiveMessage(LobbyPacket packet)
        {
            Packet = packet;
        }

        public LobbyPacket Packet;
    }
}
