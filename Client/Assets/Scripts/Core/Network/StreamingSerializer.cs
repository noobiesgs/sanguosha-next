using MemoryPack;
using MemoryPack.Internal;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System;
using System.Buffers;
using System.Diagnostics;

namespace Noobie.SanGuoSha.Network
{
    public static class StreamingSerializer
    {
        public static void Serialize<T>(MemoryStream stream, int count, IEnumerable<T> source, MemoryPackSerializerOptions options = default)
        {
            static void WriteCollectionHeader(ReusableLinkedArrayBufferWriter bufferWriter, int count, MemoryPackWriterOptionalState state)
            {
#if UNITY_5_3_OR_NEWER
                IBufferWriter<byte> byteBufferWriter = bufferWriter;
                var writer = new MemoryPackWriter(ref byteBufferWriter, state);
#else
                var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufferWriter, state);
#endif
                writer.WriteCollectionHeader(count);
                writer.Flush();
            }

            static void Write(ReusableLinkedArrayBufferWriter bufferWriter, IEnumerator<T> enumerator, MemoryPackWriterOptionalState state)
            {
#if UNITY_5_3_OR_NEWER
                IBufferWriter<byte> byteBufferWriter = bufferWriter;
                var writer = new MemoryPackWriter(ref byteBufferWriter, state);
#else
                var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref bufferWriter, state);
#endif
                while (enumerator.MoveNext())
                {
                    writer.WriteValue(enumerator.Current);
                }
                writer.Flush();
            }

            using var state = MemoryPackWriterOptionalStatePool.Rent(options);

            var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();

            try
            {
                WriteCollectionHeader(tempWriter, count, state);
                using var enumerator = source.GetEnumerator();
                Write(tempWriter, enumerator, state);
                tempWriter.WriteToAndResetAsync(stream, CancellationToken.None);
            }
            finally
            {
                ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
            }
        }

        private static IEnumerable<T> Deserialize<T>(PipeReader pipeReader, int bufferAtLeast = 4096, int readMinimumSize = 8192, MemoryPackSerializerOptions options = default)
        {
            static bool ReadCollectionHeader(in ReadOnlySequence<byte> buffer, MemoryPackReaderOptionalState state, out int length)
            {
                using var reader = new MemoryPackReader(buffer, state);
                return reader.DangerousTryReadCollectionHeader(out length);
            }

            static int InnerDeserialize(in ReadOnlySequence<byte> buffer, int bufferAtLeast, ICollection<T> itemBuffer, StrongBox<int> remain, bool bufferIsFull, MemoryPackReaderOptionalState state)
            {
                using var reader = new MemoryPackReader(buffer, state);
                while (bufferIsFull || bufferAtLeast < reader.Remaining)
                {
                    if (remain.Value == 0)
                    {
                        return reader.Consumed;
                    }

                    itemBuffer.Add(reader.ReadValue<T>());
                    remain.Value--;
                }
                return reader.Consumed;
            }

            if (readMinimumSize < bufferAtLeast)
            {
                throw new ArgumentException($"readMinimumSize must larger than bufferAtLeast. readMinimumSize: {readMinimumSize} bufferAtLeast:{bufferAtLeast}");
            }

            using var state = MemoryPackReaderOptionalStatePool.Rent(options);

            var itemBuffer = new List<T>();

            var task = pipeReader.ReadAtLeastAsync(readMinimumSize);
            Debug.Assert(task.IsCompleted);
            var readResult = task.Result;

            if (!readResult.IsCanceled)
            {
                var buffer = readResult.Buffer;
                if (ReadCollectionHeader(buffer, state, out var length))
                {
                    pipeReader.AdvanceTo(buffer.GetPosition(4));

                    if (readResult.IsCompleted)
                    {
                        buffer = buffer.Slice(4);
                    }

                    var remain = new StrongBox<int>(length);

                    while (remain.Value != 0)
                    {
                        if (!readResult.IsCompleted)
                        {
                            task = pipeReader.ReadAtLeastAsync(readMinimumSize);
                            Debug.Assert(task.IsCompleted);
                            readResult = task.Result;
                            buffer = readResult.Buffer;
                        }

                        if (readResult.IsCanceled)
                        {
                            yield break;
                        }

                        var consumedByteCount = InnerDeserialize(buffer, bufferAtLeast, itemBuffer, remain, readResult.IsCompleted, state);

                        if (itemBuffer.Count > 0)
                        {
                            foreach (var item in itemBuffer)
                            {
                                yield return item;
                            }
                            itemBuffer.Clear();
                        }

                        if (readResult.IsCompleted)
                        {
                            buffer = buffer.Slice(consumedByteCount);

                            if (consumedByteCount == 0 || buffer.Length == 0)
                            {
                                pipeReader.Complete();
                                yield break;
                            }
                        }
                        else
                        {
                            pipeReader.AdvanceTo(buffer.GetPosition(consumedByteCount));
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> Deserialize<T>(MemoryStream stream, int bufferAtLeast = 4096, int readMinimumSize = 8192, MemoryPackSerializerOptions options = default)
        {
            return Deserialize<T>(PipeReader.Create(stream), bufferAtLeast, readMinimumSize, options);
        }
    }
}
