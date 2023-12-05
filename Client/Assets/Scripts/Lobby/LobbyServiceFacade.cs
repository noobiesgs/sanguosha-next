using System;
using Noobie.SanGuoSha.Infrastructure;
using VContainer;
using VContainer.Unity;

namespace Noobie.SanGuoSha.Lobby
{
    public class LobbyServiceFacade : IDisposable, IStartable
    {
        [Inject] private readonly LifetimeScope _parentScope;
        [Inject] private readonly UpdateRunner _updateRunner;

        private LifetimeScope _serviceScope;

        public void Dispose()
        {
            _serviceScope?.Dispose();
        }

        public void Start()
        {
            _serviceScope = _parentScope.CreateChild();
        }
    }
}
