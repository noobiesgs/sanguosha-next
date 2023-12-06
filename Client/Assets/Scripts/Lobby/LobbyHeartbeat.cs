using Noobie.SanGuoSha.Infrastructure;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class LobbyHeartbeat
    {
        private readonly LocalLobbyUser _user;
        private readonly UpdateRunner _updateRunner;

        public LobbyHeartbeat(LocalLobbyUser user, UpdateRunner updateRunner)
        {
            _user = user;
            _updateRunner = updateRunner;
        }

        public void BeginTracking()
        {
            _updateRunner.Subscribe(OnUpdate, 30f);
        }

        public void EndTracking()
        {
            _updateRunner.Unsubscribe(OnUpdate);
        }

        void OnUpdate(float _)
        {
            if (!_user.IsOnline)
            {
                return;
            }

            _user.SendAsync(new PingPacket());
        }
    }
}
