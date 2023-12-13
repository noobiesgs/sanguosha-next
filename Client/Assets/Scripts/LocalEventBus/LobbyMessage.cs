using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.LocalEventBus
{
    public struct LobbyMessage
    {
        public LobbyMessage(ILobbyMessagePacket message)
        {
            Message = message;
        }

        public ILobbyMessagePacket Message;
    }
}
