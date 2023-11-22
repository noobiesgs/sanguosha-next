using System.Threading;

namespace Noobie.SanGuoSha.Utils
{
    public abstract class NumberSpace<T>
    {
        protected T Num;
        protected readonly T Default;

        protected NumberSpace(T defaultValue)
        {
            Default = defaultValue;
        }

        public T Get()
        {
            return Num;
        }

        public abstract T Increment();
    }

    public sealed class NumberSpaceInt32 : NumberSpace<int>
    {
        public override int Increment()
        {
            Interlocked.CompareExchange(ref Num, Default, int.MaxValue - 1000);
            Interlocked.Increment(ref Num);
            return Num;
        }

        public NumberSpaceInt32(int defaultValue = default) : base(defaultValue)
        {
        }
    }

    public sealed class NumberSpaceInt64 : NumberSpace<long>
    {
        public override long Increment()
        {
            Interlocked.CompareExchange(ref Num, Default, long.MaxValue - 1000);
            Interlocked.Increment(ref Num);
            return Num;
        }

        public NumberSpaceInt64(long defaultValue = default) : base(defaultValue)
        {
        }
    }
}
