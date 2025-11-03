namespace MCCS.Collecter.Memory;

/// <summary>
/// 池化的缓冲区 (RAII 模式 - 资源获取即初始化)
/// 使用 using 语句自动归还到池中
/// </summary>
public sealed class PooledBuffer : IDisposable
{
    private readonly IMemoryPool _pool;
    private bool _disposed = false;

    /// <summary>
    /// 缓冲区指针
    /// </summary>
    public IntPtr Pointer { get; }

    /// <summary>
    /// 缓冲区大小
    /// </summary>
    public int Size { get; }

    internal PooledBuffer(IntPtr pointer, int size, IMemoryPool pool)
    {
        if (pointer == IntPtr.Zero)
            throw new ArgumentException("Buffer pointer cannot be IntPtr.Zero", nameof(pointer));
        if (size <= 0)
            throw new ArgumentException("Buffer size must be positive", nameof(size));

        Pointer = pointer;
        Size = size;
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    /// <summary>
    /// 释放缓冲区（自动归还到池）
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _pool.Return(this);
            _disposed = true;
        }
    }

    /// <summary>
    /// 检查缓冲区是否已释放
    /// </summary>
    public bool IsDisposed => _disposed;
}
