using System;
using System.Buffers;

namespace Noobie.SanGuoSha.Network
{
    public class Buffer : IDisposable
    {
        private byte[] _data;
        private long _size;
        private long _offset;

        /// <summary>
        /// Is the buffer empty?
        /// </summary>
        public bool IsEmpty => _size == 0;
        /// <summary>
        /// Bytes memory buffer
        /// </summary>
        public byte[] Data => _data;
        /// <summary>
        /// Bytes memory buffer capacity
        /// </summary>
        public long Capacity => _data.Length;
        /// <summary>
        /// Bytes memory buffer size
        /// </summary>
        public long Size => _size;
        /// <summary>
        /// Bytes memory buffer offset
        /// </summary>
        public long Offset => _offset;

        /// <summary>
        /// Buffer indexer operator
        /// </summary>
        public byte this[long index] => _data[index];

        /// <summary>
        /// Initialize a new expandable buffer with zero capacity
        /// </summary>
        public Buffer() : this(0) { }

        /// <summary>
        /// Initialize a new expandable buffer with the given capacity
        /// </summary>
        public Buffer(long capacity)
        {
            _data = ArrayPool<byte>.Shared.Rent((int)capacity);
            _size = 0;
            _offset = 0;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_data);
        }

        // Clear the current buffer and its offset
        public void Clear()
        {
            _size = 0;
            _offset = 0;
        }


        /// <summary>
        /// Reserve the buffer of the given capacity
        /// </summary>
        public void Reserve(long capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("Invalid reserve capacity!", nameof(capacity));

            if (capacity > Capacity)
            {
                var data = ArrayPool<byte>.Shared.Rent((int)Math.Max(capacity, 2 * Capacity));
                _data.AsSpan().CopyTo(data.AsSpan(0, (int)_size));
                ArrayPool<byte>.Shared.Return(_data);
                _data = data;
            }
        }

        /// <summary>
        /// Append the given span of bytes
        /// </summary>
        /// <param name="buffer">Buffer to append as a span of bytes</param>
        /// <returns>Count of append bytes</returns>
        public long Append(ReadOnlySpan<byte> buffer)
        {
            Reserve(_size + buffer.Length);
            buffer.CopyTo(new Span<byte>(_data, (int)_size, buffer.Length));
            _size += buffer.Length;
            return buffer.Length;
        }

        /// <summary>
        /// Append the given span of bytes
        /// </summary>
        /// <param name="buffer">Buffer to append as a sequence of bytes</param>
        /// <returns>Count of append bytes</returns>
        public long Append(ReadOnlySequence<byte> buffer)
        {
            Reserve(_size + buffer.Length);
            buffer.CopyTo(new Span<byte>(_data, (int)_size, (int)buffer.Length));
            _size += buffer.Length;
            return buffer.Length;
        }
    }
}
