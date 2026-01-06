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
    private bool _disposed = false;
    private bool _isCreator = false;

    // 环形缓冲区头部结构（12字节）
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct RingBufferHeader
    {
        public int WriteIndex;  // 写指针
        public int ReadIndex;   // 读指针
        public int Count;       // 当前数据个数
    }

    /// <summary>
    /// 初始化共享内存客户端
    /// </summary>
    /// <param name="memoryName">共享内存名称</param>
    /// <param name="itemSize">单个数据项大小</param>
    /// <param name="maxItems">最大数据项数量</param>
    public SharedMemoryClient(string memoryName, int maxItems = 60, int itemSize = 4096)
    {
        _memoryName = memoryName;
        _mutexName = $"{memoryName}_Mutex";
        _itemSize = itemSize;
        _maxItems = maxItems;
        _memorySize = Marshal.SizeOf<RingBufferHeader>() + (_itemSize * _maxItems);
        Initialize();
    }

    /// <summary>
    /// 初始化共享内存资源
    /// </summary>
    private void Initialize()
    {
        try
        {
            // 使用原来的简单方式创建互斥量
            _mutex = new Mutex(false, _mutexName);
            // 先尝试打开已存在的共享内存
            try
            {
                _mmf = MemoryMappedFile.OpenExisting(_memoryName);
                _isCreator = false;
            }
            catch (FileNotFoundException)
            {
                // 不存在，则创建新的
                _mmf = MemoryMappedFile.CreateOrOpen(_memoryName, _memorySize);
                _isCreator = true;
            } 
            // 统一使用不指定大小的方式创建访问器
            _accessor = _mmf.CreateViewAccessor();

            // 如果是创建者，初始化环形缓冲区头部
            if (_isCreator)
            {
                InitializeRingBuffer();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"初始化共享内存失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 初始化环形缓冲区
    /// </summary>
    private void InitializeRingBuffer()
    {
        _mutex.WaitOne();
        try
        {
            var header = new RingBufferHeader
            {
                WriteIndex = 0,
                ReadIndex = 0,
                Count = 0
            };
            WriteHeader(header);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// 写入环形缓冲区头部
    /// </summary>
    private void WriteHeader(RingBufferHeader header)
    {
        var headerBytes = StructToBytes(header);
        _accessor.WriteArray(0, headerBytes, 0, headerBytes.Length);
    }

    /// <summary>
    /// 读取环形缓冲区头部
    /// </summary>
    private RingBufferHeader ReadHeader()
    {
        var headerSize = Marshal.SizeOf<RingBufferHeader>();
        var headerBytes = new byte[headerSize];
        _accessor.ReadArray(0, headerBytes, 0, headerSize);
        return BytesToStruct<RingBufferHeader>(headerBytes);
    }

    /// <summary>
    /// 通用发送数据方法
    /// </summary>
    public void SendData<T>(T data) where T : struct
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SharedMemoryClient));

        var dataBytes = StructToBytes(data);
        if (dataBytes.Length > _itemSize)
        {
            throw new InvalidOperationException($"数据大小 ({dataBytes.Length}) 超过限制 ({_itemSize})");
        }

        _mutex.WaitOne();
        try
        {
            var header = ReadHeader();

            // 如果缓冲区已满，覆盖最老的数据
            if (header.Count >= _maxItems)
            {
                header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
                header.Count = _maxItems - 1;
            }

            // 写入数据
            var dataOffset = Marshal.SizeOf<RingBufferHeader>() + (header.WriteIndex * _itemSize);
            _accessor.WriteArray(dataOffset, dataBytes, 0, dataBytes.Length);

            // 更新头部
            header.WriteIndex = (header.WriteIndex + 1) % _maxItems;
            header.Count++;
            WriteHeader(header);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// 通用读取数据方法
    /// </summary>
    public T ReadData<T>(bool movePointer = true) where T : struct
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SharedMemoryClient));
        _mutex.WaitOne();
        try
        {
            var header = ReadHeader();
            // 没有数据时返回默认值
            if (header.Count == 0)
            {
                return default;
            }
            // 读取数据
            int dataOffset = Marshal.SizeOf<RingBufferHeader>() + (header.ReadIndex * _itemSize);
            byte[] dataBytes = new byte[_itemSize];
            _accessor.ReadArray(dataOffset, dataBytes, 0, _itemSize);
            var data = BytesToStruct<T>(dataBytes);

            // 只有在 movePointer 为 true 时才更新头部
            if (movePointer)
            {
                // 更新头部
                header.ReadIndex = (header.ReadIndex + 1) % _maxItems;
                header.Count--;
                WriteHeader(header);
            }

            return data;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// 尝试读取数据（非阻塞，带超时）
    /// </summary>
    public bool TryReadData<T>(out T data, int timeoutMs = 100) where T : struct
    {
        data = default(T);
        if (_disposed) return false;

        if (!_mutex.WaitOne(timeoutMs))
        {
            return false;
        }

        try
        {
            var header = ReadHeader();
            if (header.Count == 0)
            {
                return false;
            }

            data = ReadData<T>();
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// 获取缓冲区状态
    /// </summary>
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

    /// <summary>
    /// 结构体转字节数组
    /// </summary>
    private static byte[] StructToBytes<T>(T structure) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var bytes = new byte[size];
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    /// <summary>
    /// 字节数组转结构体
    /// </summary>
    private static T BytesToStruct<T>(byte[] bytes) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        if (bytes.Length < size)
            throw new ArgumentException($"Array ({bytes.Length}) < ({size})");

        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(bytes, 0, ptr, size);
            return Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
            _mutex?.Dispose();
            _disposed = true;
        }
    }
}
