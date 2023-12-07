using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.LocalEventBus
{
    public struct LobbyPacketReceivedMessage
    {
        public LobbyPacketReceivedMessage(LobbyPacket packet)
        {
            Packet = packet;
        }

        public LobbyPacket Packet;
    }
}
