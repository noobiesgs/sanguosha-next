﻿using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Noobie.SanGuoSha.LocalEventBus
{
    internal class MessageChannel<T> : IMessageChannel<T>
    {
        private readonly List<Action<T>> _messageHandlers = new();
        private readonly Dictionary<Action<T>, bool> _pendingHandlers = new();

        public virtual void Publish(T message)
        {
            foreach (var handler in _pendingHandlers.Keys)
            {
                if (_pendingHandlers[handler])
                {
                    _messageHandlers.Add(handler);
                }
                else
                {
                    _messageHandlers.Remove(handler);
                }
            }
            _pendingHandlers.Clear();

            foreach (var messageHandler in _messageHandlers)
            {
                messageHandler?.Invoke(message);
            }
        }

        public virtual IDisposable Subscribe(Action<T> handler)
        {
            Assert.IsTrue(!IsSubscribed(handler), "Attempting to subscribe with the same handler more than once");

            if (_pendingHandlers.ContainsKey(handler))
            {
                if (!_pendingHandlers[handler])
                {
                    _pendingHandlers.Remove(handler);
                }
            }
            else
            {
                _pendingHandlers[handler] = true;
            }

            var subscription = new DisposableSubscription<T>(this, handler);
            return subscription;
        }

        public void Unsubscribe(Action<T> handler)
        {
            if (IsSubscribed(handler))
            {
                if (_pendingHandlers.ContainsKey(handler))
                {
                    if (_pendingHandlers[handler])
                    {
                        _pendingHandlers.Remove(handler);
                    }
                }
                else
                {
                    _pendingHandlers[handler] = false;
                }
            }
        }

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            _messageHandlers.Clear();
            _pendingHandlers.Clear();
        }

        public bool IsDisposed { get; private set; }

        private bool IsSubscribed(Action<T> handler)
        {
            var isPendingRemoval = _pendingHandlers.ContainsKey(handler) && !_pendingHandlers[handler];
            var isPendingAdding = _pendingHandlers.ContainsKey(handler) && _pendingHandlers[handler];
            return _messageHandlers.Contains(handler) && !isPendingRemoval || isPendingAdding;
        }
    }
}
