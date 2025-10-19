using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MCCS.Collecter.DllNative
{
    public class BufferPool
    {
        private static readonly ConcurrentBag<IntPtr> _pool = [];
        private static readonly int _bufferSize = 1024 * 1024; // 1MB固定大小

        public static IntPtr Rent()
        {
            if (_pool.TryTake(out IntPtr buffer))
            {
                return buffer;
            }
            return Marshal.AllocHGlobal(_bufferSize);
        }

        public static void Return(IntPtr buffer)
        {
            if (buffer != IntPtr.Zero)
            {
                _pool.Add(buffer);
            }
        }

        public static void Clear()
        {
            while (_pool.TryTake(out var buffer))
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
