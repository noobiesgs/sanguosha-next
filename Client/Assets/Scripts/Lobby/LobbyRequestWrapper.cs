using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Noobie.SanGuoSha.Network;

namespace Noobie.SanGuoSha.Lobby
{
    public class LobbyRequestWrapper
    {
        private readonly ConcurrentDictionary<int, UniTaskCompletionSource<ILobbyResponsePacket>> _completionSources = new();

        public void Add(int requestId, UniTaskCompletionSource<ILobbyResponsePacket> utcs, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                utcs.TrySetCanceled(cancellationToken);
                return;
            }
            cancellationToken.Register(() =>
            {
                if (_completionSources.TryRemove(requestId, out var completionSource))
                {
                    completionSource.TrySetCanceled(cancellationToken);
                }
            });

            _completionSources.TryAdd(requestId, utcs);
        }

        public void Response(ILobbyResponsePacket response)
        {
            if (_completionSources.TryRemove(response.ResponseId, out var completionSource))
            {
                completionSource.TrySetResult(response);
            }
        }

        public void Clear()
        {
            foreach (var key in _completionSources.Keys.ToList())
            {
                if (_completionSources.TryRemove(key, out var completionSource))
                {
                    completionSource.TrySetException(new Exception("Client disconnected"));
                }
            }
        }
    }
}
