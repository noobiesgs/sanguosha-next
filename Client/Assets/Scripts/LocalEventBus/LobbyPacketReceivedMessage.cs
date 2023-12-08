using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.LocalEventBus
{
    public struct LobbyPacketReceivedMessage
    {
        public LobbyPacketReceivedMessage(ILobbyPacket packet)
        {
            Packet = packet;
        }

        public ILobbyPacket Packet;
    }
}
