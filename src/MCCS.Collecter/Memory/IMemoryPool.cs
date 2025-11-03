namespace MCCS.Collecter.Memory;

/// <summary>
/// 内存池接口
/// </summary>
public interface IMemoryPool : IDisposable
{
    /// <summary>
    /// 租用缓冲区
    /// </summary>
    /// <param name="size">需要的缓冲区大小</param>
    /// <returns>池化的缓冲区（使用 RAII 模式自动归还）</returns>
    PooledBuffer Rent(int size);

    /// <summary>
    /// 归还缓冲区
    /// </summary>
    /// <param name="buffer">要归还的缓冲区</param>
    void Return(PooledBuffer buffer);

    /// <summary>
    /// 清空池中所有缓冲区
    /// </summary>
    void Clear();

    /// <summary>
    /// 获取池中的统计信息
    /// </summary>
    MemoryPoolStats GetStats();
}

/// <summary>
/// 内存池统计信息
/// </summary>
public record MemoryPoolStats
{
    /// <summary>
    /// 池中可用缓冲区数量
    /// </summary>
    public int AvailableBuffers { get; init; }

    /// <summary>
    /// 已租出的缓冲区数量
    /// </summary>
    public int RentedBuffers { get; init; }

    /// <summary>
    /// 总分配的内存大小 (字节)
    /// </summary>
    public long TotalAllocatedBytes { get; init; }
}
