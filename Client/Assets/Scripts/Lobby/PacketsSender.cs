using System.Collections.Generic;
using Microsoft.IO;
using Noobie.SanGuoSha.Infrastructure;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class PacketsSender
    {
        private readonly UpdateRunner _updateRunner;
        private readonly LocalLobbyUser _user;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

        public PacketsSender(UpdateRunner updateRunner, LocalLobbyUser user)
        {
            _updateRunner = updateRunner;
            _user = user;
        }

        public void BeginSend()
        {
            _updateRunner.Subscribe(OnUpdate, 1f / 60);
        }

        public void EndSend()
        {
            _updateRunner.Unsubscribe(OnUpdate);
        }

        private void OnUpdate(float _)
        {
            if (_user.Packets.Count == 0 || !_user.IsOnline)
            {
                return;
            }

            var packets = new List<GameDataPacket>();
            while (_user.Packets.TryDequeue(out var packet))
            {
                packets.Add(packet);
            }

            using var memory = (RecyclableMemoryStream)_memoryStreamManager.GetStream();
            StreamingSerializer.Serialize(memory, packets.Count, packets);
            _user.Connection?.Send(memory.GetReadOnlySequence());
        }
    }
}
