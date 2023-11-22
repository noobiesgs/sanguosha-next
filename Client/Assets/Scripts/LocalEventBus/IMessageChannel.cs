using System;

namespace Noobie.SanGuoSha.LocalEventBus
{
    internal interface IPublisher<in T>
    {
        void Publish(T message);
    }

    internal interface ISubscriber<out T>
    {
        IDisposable Subscribe(Action<T> handler);

        void Unsubscribe(Action<T> handler);
    }

    internal interface IMessageChannel<T> : IPublisher<T>, ISubscriber<T>, IDisposable
    {
        bool IsDisposed { get; }
    }

    internal interface IBufferedMessageChannel<T> : IMessageChannel<T>
    {
        bool HasBufferedMessage { get; }

        T BufferedMessage { get; }
    }
}
