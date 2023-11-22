using System;
using System.Collections.Generic;

namespace Noobie.SanGuoSha.Infrastructure
{
    internal class DisposableGroup : IDisposable
    {
        private readonly List<IDisposable> _disposables = new();

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }

        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }
    }
}
