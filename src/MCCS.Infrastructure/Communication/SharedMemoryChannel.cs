using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MCCS.Infrastructure.Communication;

/// <summary>
/// 共享内存通道接口 - 定义基本的读写操作
/// </summary>
public interface ISharedMemoryChannel : IDisposable
{
    /// <summary>
    /// 通道名称
    /// </summary>
    string ChannelName { get; }

    /// <summary>
    /// 是否已初始化
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 获取当前缓冲区状态
    /// </summary>
    (int count, int capacity) GetBufferStatus();
}

/// <summary>
/// 共享内存写入通道接口
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public interface ISharedMemoryWriter<T> : ISharedMemoryChannel where T : struct
{
    /// <summary>
    /// 写入单个数据项
    /// </summary>
    void Write(ref T data);

    /// <summary>
    /// 批量写入数据
    /// </summary>
    void WriteBatch(IEnumerable<T> dataItems);

    /// <summary>
    /// 尝试写入数据（非阻塞）
    /// </summary>
    bool TryWrite(T data, int timeoutMs = 100);
}

/// <summary>
/// 共享内存读取通道接口
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public interface ISharedMemoryReader<T> : ISharedMemoryChannel where T : struct
{
    /// <summary>
    /// 读取单个数据项
    /// </summary>
    T Read();

    /// <summary>
    /// 批量读取数据
    /// </summary>
    List<T> ReadBatch(int maxCount = -1);

    /// <summary>
    /// 尝试读取数据（非阻塞）
    /// </summary>
    bool TryRead(out T data, int timeoutMs = 100);

    /// <summary>
    /// 查看下一个数据项但不移除
    /// </summary>
    T Peek();
}

/// <summary>
/// 高性能共享内存通道实现
/// 支持泛型数据类型，使用环形缓冲区
/// </summary>
/// <typeparam name="T">数据结构类型（必须是值类型）</typeparam>
public sealed class SharedMemoryChannel<T> : ISharedMemoryWriter<T>, ISharedMemoryReader<T> where T : struct
{
    private readonly string _channelName;
    private readonly string _mutexName;
    private readonly int _maxItems;
    private readonly int _itemSize;
    private readonly int _memorySize;

    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _accessor;
    private Mutex? _mutex;
    private bool _disposed;
    private bool _isCreator;

    // ReSharper disable once InconsistentNaming
    private static readonly int _headerSize = Unsafe.SizeOf<RingBufferHeader>();

    [StructLayout(LayoutKind.Sequential)]
    private struct RingBufferHeader
    {
        public int WriteIndex;
        public int ReadIndex;
        public int Count;
    }

    public string ChannelName => _channelName;
    public bool IsInitialized => _mmf != null && !_disposed;
    public bool IsCreator => _isCreator;

    /// <summary>
    /// 创建或打开共享内存通道
    /// </summary>
    /// <param name="channelName">通道名称</param>
    /// <param name="maxItems">最大项数</param>
    /// <param name="itemSize">单项大小（如果为0则自动计算）</param>
    public SharedMemoryChannel(string channelName, int maxItems = 1000, int itemSize = 0)
    {
        _channelName = channelName;
        _mutexName = $"{channelName}_Mutex";
        _maxItems = maxItems;
        _itemSize = itemSize > 0 ? itemSize : Marshal.SizeOf<T>();
        _memorySize = _headerSize + _itemSize * _maxItems;

        Initialize();
    }

    private void Initialize()
    {
        _mutex = new Mutex(false, _mutexName);

        try
        {
            _mmf = MemoryMappedFile.OpenExisting(_channelName);
            _isCreator = false;
        }
        catch (FileNotFoundException)
        {
            _mmf = MemoryMappedFile.CreateOrOpen(_channelName, _memorySize);
            _isCreator = true;
        }

        _accessor = _mmf.CreateViewAccessor();

        if (_isCreator)
        {
            InitializeBuffer();
        }
    }

    private void InitializeBuffer()
    {
        _mutex!.WaitOne();
        try
        {
            var header = new RingBufferHeader
            {
                WriteIndex = 0,
                ReadIndex = 0,
                Count = 0
            };
            _accessor!.Write(0, ref header);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    private RingBufferHeader ReadHeader()
    {
        _accessor!.Read(0, out RingBufferHeader header);
        return header;
    }

    private void WriteHeader(ref RingBufferHeader header)
    {
        _accessor!.Write(0, ref header);
    }

    #region ISharedMemoryWriter<T> Implementation

    public void Write(ref T data)
    {
        EnsureNotDisposed();

        _mutex!.WaitOne();
        try
        {
            var header = ReadHeader();

            // 如果缓冲区已满，覆盖最旧的数据
            if (header.Count == _maxItems)
            {
                header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
                header.Count--;
            }

            long offset = _headerSize + header.WriteIndex * _itemSize;
            _accessor!.Write(offset, ref data);

            header.WriteIndex = (header.WriteIndex + 1) % _maxItems;
            header.Count++;  
            WriteHeader(ref header);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public void WriteBatch(IEnumerable<T> dataItems)
    {
        EnsureNotDisposed();

        _mutex!.WaitOne();
        try
        {
            var header = ReadHeader();
            var items = dataItems.ToList();

            foreach (var data in items)
            {
                if (header.Count == _maxItems)
                {
                    header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
                    header.Count--;
                }

                long offset = _headerSize + header.WriteIndex * _itemSize;
                var item = data;
                _accessor!.Write(offset, ref item);

                header.WriteIndex = (header.WriteIndex + 1) % _maxItems;
                header.Count++; 
            } 
            WriteHeader(ref header);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public bool TryWrite(T data, int timeoutMs = 100)
    {
        if (_disposed) return false;

        if (!_mutex!.WaitOne(timeoutMs))
            return false;

        try
        {
            var header = ReadHeader();

            if (header.Count == _maxItems)
            {
                header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
                header.Count--;
            }

            long offset = _headerSize + header.WriteIndex * _itemSize;
            _accessor!.Write(offset, ref data);

            header.WriteIndex = (header.WriteIndex + 1) % _maxItems;
            header.Count++; 

            WriteHeader(ref header);
            return true;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    #endregion

    #region ISharedMemoryReader<T> Implementation

    public T Read()
    {
        EnsureNotDisposed();

        _mutex!.WaitOne();
        try
        {
            var header = ReadHeader();
            if (header.Count == 0)
                return default;

            long offset = _headerSize + header.ReadIndex * _itemSize;
            _accessor!.Read(offset, out T data);

            header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
            header.Count--;
            WriteHeader(ref header);

            return data;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public List<T> ReadBatch(int maxCount = -1)
    {
        EnsureNotDisposed();

        _mutex!.WaitOne();
        try
        {
            var header = ReadHeader();
            if (header.Count == 0)
                return new List<T>(0);

            int available = header.Count;
            int readCount = (maxCount < 0 || maxCount > available) ? available : maxCount;
            var result = new List<T>(readCount);

            int index = header.ReadIndex;
            for (int i = 0; i < readCount; i++)
            {
                long offset = _headerSize + index * _itemSize;
                _accessor!.Read(offset, out T item);
                result.Add(item);

                index = (index + 1) % _maxItems;
            }

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

    public bool TryRead(out T data, int timeoutMs = 100)
    {
        data = default;

        if (_disposed)
            return false;

        if (!_mutex!.WaitOne(timeoutMs))
            return false;

        try
        {
            var header = ReadHeader();
            if (header.Count == 0)
                return false;

            long offset = _headerSize + header.ReadIndex * _itemSize;
            _accessor!.Read(offset, out data);

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

    public T Peek()
    {
        EnsureNotDisposed();

        _mutex!.WaitOne();
        try
        {
            var header = ReadHeader();
            if (header.Count == 0)
                return default;

            long offset = _headerSize + header.ReadIndex * _itemSize;
            _accessor!.Read(offset, out T data);

            return data;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    #endregion

    public (int count, int capacity) GetBufferStatus()
    {
        if (_disposed) return (0, 0);

        _mutex!.WaitOne();
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

    /// <summary>
    /// 清空缓冲区
    /// </summary>
    public void Clear()
    {
        EnsureNotDisposed();

        _mutex!.WaitOne();
        try
        {
            var header = ReadHeader();
            header.ReadIndex = 0;
            header.WriteIndex = 0;
            header.Count = 0;
            WriteHeader(ref header);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SharedMemoryChannel<T>));
    }

    public void Dispose()
    {
        if (_disposed) return;

        _accessor?.Dispose();
        _mmf?.Dispose();
        _mutex?.Dispose();

        _accessor = null;
        _mmf = null;
        _mutex = null;

        _disposed = true;
    }
}
