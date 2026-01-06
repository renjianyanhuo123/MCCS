using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace MCCS.Infrastructure.Helper;

public sealed class SharedMemoryClient : IDisposable
{
    private readonly string _memoryName;
    private readonly string _mutexName;
    private readonly int _memorySize;
    private readonly int _itemSize;
    private readonly int _maxItems;

    private MemoryMappedFile _mmf;
    private MemoryMappedViewAccessor _accessor;
    private Mutex _mutex;
    private bool _disposed;
    private bool _isCreator;

    private static readonly int HeaderSize = Marshal.SizeOf<RingBufferHeader>();

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct RingBufferHeader
    {
        public int WriteIndex;
        public int ReadIndex;
        public int Count;
    }

    public SharedMemoryClient(string memoryName, int maxItems = 60, int itemSize = 4096)
    {
        _memoryName = memoryName;
        _mutexName = $"{memoryName}_Mutex";
        _itemSize = itemSize;
        _maxItems = maxItems;
        _memorySize = HeaderSize + _itemSize * _maxItems;
        Initialize();
    }

    private void Initialize()
    {
        _mutex = new Mutex(false, _mutexName);

        try
        {
            _mmf = MemoryMappedFile.OpenExisting(_memoryName);
            _isCreator = false;
        }
        catch (FileNotFoundException)
        {
            _mmf = MemoryMappedFile.CreateOrOpen(_memoryName, _memorySize);
            _isCreator = true;
        }

        _accessor = _mmf.CreateViewAccessor();

        if (_isCreator)
            InitializeRingBuffer();
    }

    private void InitializeRingBuffer()
    {
        _mutex.WaitOne();
        try
        {
            var header = new RingBufferHeader();
            _accessor.Write(0, ref header);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    private RingBufferHeader ReadHeader()
    {
        _accessor.Read(0, out RingBufferHeader header);
        return header;
    }

    private void WriteHeader(ref RingBufferHeader header)
    {
        _accessor.Write(0, ref header);
    }

    /// <summary>
    /// 批量读取数据
    /// </summary>
    /// <typeparam name="T">数据结构类型</typeparam>
    /// <param name="maxCount">最大读取数量，-1表示读取所有可用数据</param>
    /// <returns>读取到的数据列表</returns>
    public List<T> ReadDataBatch<T>(int maxCount = -1) where T : struct
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SharedMemoryClient));

        _mutex.WaitOne();
        try
        {
            var header = ReadHeader();
            if (header.Count == 0)
                return new List<T>(0);

            int available = header.Count;
            int readCount = (maxCount < 0 || maxCount > available)
                ? available
                : maxCount;

            // 预分配容量，避免 List 扩容
            var result = new List<T>(readCount);

            int index = header.ReadIndex;

            for (int i = 0; i < readCount; i++)
            {
                long offset = HeaderSize + index * _itemSize;
                _accessor.Read(offset, out T item);
                result.Add(item);

                index++;
                if (index == _maxItems)
                    index = 0;
            }

            // 更新头部（一次性）
            header.ReadIndex = index;
            header.Count -= readCount;
            WriteHeader(ref header);

            return result;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    } 

    /// <summary>
    /// 读取单个数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="movePointer"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public T ReadData<T>(bool movePointer = true) where T : struct
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SharedMemoryClient));

        _mutex.WaitOne();
        try
        {
            var header = ReadHeader();
            if (header.Count == 0)
                return default;

            long offset = HeaderSize + header.ReadIndex * _itemSize;
            _accessor.Read(offset, out T data);

            if (movePointer)
            {
                header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
                header.Count--;
                WriteHeader(ref header);
            }

            return data;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }
    /// <summary>
    /// 发送数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void SendData<T>(T data) where T : struct
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SharedMemoryClient));

        var dataSize = Marshal.SizeOf<T>();
        if (dataSize > _itemSize)
            throw new InvalidOperationException($"数据大小 {dataSize} 超过限制 {_itemSize}");

        _mutex.WaitOne();
        try
        {
            var header = ReadHeader();

            if (header.Count == _maxItems)
            {
                header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
                header.Count--;
            }

            long offset = HeaderSize + header.WriteIndex * _itemSize;
            _accessor.Write(offset, ref data);

            header.WriteIndex = (header.WriteIndex + 1) % _maxItems;
            header.Count++;

            WriteHeader(ref header);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }
    /// <summary> 
    /// 尝试读取数据（非阻塞，带超时） 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="timeoutMs"></param>
    /// <returns></returns>
    public bool TryReadData<T>(out T data, int timeoutMs = 100) where T : struct
    {
        data = default;

        if (_disposed)
            return false;

        if (!_mutex.WaitOne(timeoutMs))
            return false;

        try
        {
            var header = ReadHeader();
            if (header.Count == 0)
                return false;

            long offset = HeaderSize + header.ReadIndex * _itemSize;
            _accessor.Read(offset, out data);

            header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
            header.Count--;
            WriteHeader(ref header);

            return true;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }
    /// <summary>
    /// 获取当前缓冲区的状态
    /// </summary>
    /// <returns></returns>
    public (int count, int capacity) GetBufferStatus()
    {
        if (_disposed) return (0, 0);

        _mutex.WaitOne();
        try
        {
            var header = ReadHeader();
            return (header.Count, _maxItems);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _accessor?.Dispose();
        _mmf?.Dispose();
        _mutex?.Dispose();
        _disposed = true;
    }
}

