using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MCCS.Collecter.Memory;

/// <summary>
/// 优化的内存池实现
/// 支持多种缓冲区大小，自动管理生命周期
/// </summary>
public class OptimizedMemoryPool : IMemoryPool
{
    private readonly ConcurrentDictionary<int, ConcurrentBag<IntPtr>> _pools = new();
    private readonly int[] _standardSizes;
    private readonly int _maxPoolSize;
    private int _rentedCount = 0;
    private long _totalAllocatedBytes = 0;
    private bool _disposed = false;

    /// <summary>
    /// 创建优化的内存池
    /// </summary>
    /// <param name="standardSizes">标准缓冲区大小数组（默认：1KB, 4KB, 16KB, 64KB, 1MB）</param>
    /// <param name="maxPoolSize">每个大小的最大池数量（防止内存泄漏）</param>
    public OptimizedMemoryPool(int[]? standardSizes = null, int maxPoolSize = 100)
    {
        _standardSizes = standardSizes ?? new[] { 1024, 4096, 16384, 65536, 1048576 };
        _maxPoolSize = maxPoolSize;
    }

    /// <summary>
    /// 租用缓冲区
    /// </summary>
    public PooledBuffer Rent(int size)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OptimizedMemoryPool));

        if (size <= 0)
            throw new ArgumentException("Size must be positive", nameof(size));

        // 找到最接近的标准尺寸
        var standardSize = FindStandardSize(size);

        // 获取或创建对应大小的池
        var pool = _pools.GetOrAdd(standardSize, _ => new ConcurrentBag<IntPtr>());

        IntPtr pointer;

        // 尝试从池中获取
        if (pool.TryTake(out pointer))
        {
            // 从池中获取到缓冲区
            Interlocked.Increment(ref _rentedCount);
            return new PooledBuffer(pointer, standardSize, this);
        }

        // 池中没有可用的，分配新的
        pointer = Marshal.AllocHGlobal(standardSize);
        Interlocked.Add(ref _totalAllocatedBytes, standardSize);
        Interlocked.Increment(ref _rentedCount);

        return new PooledBuffer(pointer, standardSize, this);
    }

    /// <summary>
    /// 归还缓冲区
    /// </summary>
    public void Return(PooledBuffer buffer)
    {
        if (buffer == null || buffer.Pointer == IntPtr.Zero)
            return;

        if (_disposed)
        {
            // 池已释放，直接释放内存
            Marshal.FreeHGlobal(buffer.Pointer);
            return;
        }

        Interlocked.Decrement(ref _rentedCount);

        var pool = _pools.GetOrAdd(buffer.Size, _ => new ConcurrentBag<IntPtr>());

        // 检查池大小，防止内存泄漏
        if (pool.Count < _maxPoolSize)
        {
            pool.Add(buffer.Pointer);
        }
        else
        {
            // 池已满，释放内存
            Marshal.FreeHGlobal(buffer.Pointer);
            Interlocked.Add(ref _totalAllocatedBytes, -buffer.Size);
        }
    }

    /// <summary>
    /// 清空所有池
    /// </summary>
    public void Clear()
    {
        foreach (var pool in _pools.Values)
        {
            while (pool.TryTake(out var pointer))
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        _pools.Clear();
        _totalAllocatedBytes = 0;
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public MemoryPoolStats GetStats()
    {
        var availableBuffers = _pools.Values.Sum(p => p.Count);

        return new MemoryPoolStats
        {
            AvailableBuffers = availableBuffers,
            RentedBuffers = _rentedCount,
            TotalAllocatedBytes = _totalAllocatedBytes
        };
    }

    /// <summary>
    /// 找到最接近的标准大小
    /// </summary>
    private int FindStandardSize(int requestedSize)
    {
        // 找到第一个大于等于请求大小的标准尺寸
        var standardSize = _standardSizes.FirstOrDefault(s => s >= requestedSize);

        if (standardSize == 0)
        {
            // 没有找到标准尺寸，使用请求的大小
            // 向上对齐到 1KB
            standardSize = ((requestedSize + 1023) / 1024) * 1024;
        }

        return standardSize;
    }

    /// <summary>
    /// 释放所有资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _disposed = true;
        }
    }
}
