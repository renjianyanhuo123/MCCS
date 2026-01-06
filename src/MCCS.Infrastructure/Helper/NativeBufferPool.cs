using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MCCS.Infrastructure.Helper
{
    public sealed class NativeBuffer
    {
        public IntPtr Ptr { get; internal set; }
        public int Size { get; internal set; }

        internal NativeBuffer(IntPtr ptr, int size)
        {
            Ptr = ptr;
            Size = size;
        }
    }

    public static class NativeBufferPool
    {
        // === 可调参数 ===
        private static readonly int[] _bucketSizes =
        [
            64,
            256,
            1024,
            8 * 1024,
            64 * 1024,
            256 * 1024,
            1024 * 1024
        ];

        private const int _maxPerBucket = 32;

        private sealed class Bucket(int size)
        {
            public readonly int Size = size;
            public readonly ConcurrentStack<NativeBuffer> Pool = new();
            public int Count;
        }

        private static readonly Bucket[] _buckets;

        static NativeBufferPool()
        {
            _buckets = new Bucket[_bucketSizes.Length];
            for (var i = 0; i < _bucketSizes.Length; i++)
            {
                _buckets[i] = new Bucket(_bucketSizes[i]);
            }
        }

        // === 公共 API ===

        public static NativeBuffer Rent(int minSize)
        {
            var bucket = SelectBucket(minSize);
            if (bucket == null)
            {
                // 超大 buffer：不入池，直接分配
                var ptr = Marshal.AllocHGlobal(minSize);
                return new NativeBuffer(ptr, minSize);
            }

            if (bucket.Pool.TryPop(out var buffer))
            {
                Interlocked.Decrement(ref bucket.Count);
                return buffer;
            }

            var newPtr = Marshal.AllocHGlobal(bucket.Size);
            return new NativeBuffer(newPtr, bucket.Size);
        }

        public static void Return(NativeBuffer buffer)
        {
            if (buffer == null || buffer.Ptr == IntPtr.Zero)
                return;

            var bucket = SelectBucket(buffer.Size);
            if (bucket == null || buffer.Size != bucket.Size)
            {
                // 非池化 buffer
                Marshal.FreeHGlobal(buffer.Ptr);
                return;
            }

            if (Interlocked.Increment(ref bucket.Count) <= _maxPerBucket)
            {
                bucket.Pool.Push(buffer);
            }
            else
            {
                Interlocked.Decrement(ref bucket.Count);
                Marshal.FreeHGlobal(buffer.Ptr);
            }
        }

        public static void Clear()
        {
            foreach (var bucket in _buckets)
            {
                while (bucket.Pool.TryPop(out var buffer))
                {
                    Marshal.FreeHGlobal(buffer.Ptr);
                }
                bucket.Count = 0;
            }
        }

        // === 内部逻辑 ===

        private static Bucket? SelectBucket(int size)
        {
            for (var i = 0; i < _buckets.Length; i++)
            {
                if (size <= _buckets[i].Size)
                    return _buckets[i];
            }
            return null;
        }
    }
}
