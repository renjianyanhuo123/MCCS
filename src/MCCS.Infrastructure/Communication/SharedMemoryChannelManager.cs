using System.Collections.Concurrent;

namespace MCCS.Infrastructure.Communication;

/// <summary>
/// 共享内存通道管理器 - 管理多个共享内存通道
/// </summary>
public sealed class SharedMemoryChannelManager : IDisposable
{
    private readonly ConcurrentDictionary<string, IDisposable> _channels = new();
    private bool _disposed;

    /// <summary>
    /// 数据通道名称前缀
    /// </summary>
    public const string DataChannelPrefix = "MCCS_Data_";

    /// <summary>
    /// 状态通道名称
    /// </summary>
    public const string StatusChannelName = "MCCS_Status";

    /// <summary>
    /// 命令通道名称
    /// </summary>
    public const string CommandChannelName = "MCCS_Command";

    /// <summary>
    /// 获取或创建泛型通道
    /// </summary>
    public SharedMemoryChannel<T> GetOrCreateChannel<T>(string channelName, int maxItems = 1000) where T : struct
    {
        EnsureNotDisposed();

        return (SharedMemoryChannel<T>)_channels.GetOrAdd(channelName,
            name => new SharedMemoryChannel<T>(name, maxItems));
    }

    /// <summary>
    /// 获取或创建数据通道
    /// </summary>
    public SharedMemoryChannel<T> GetOrCreateDataChannel<T>(string channelName, int maxItems = 1000) where T : struct
    {
        EnsureNotDisposed();

        var fullName = $"{DataChannelPrefix}{channelName}";
        return GetOrCreateChannel<T>(fullName, maxItems);
    }

    /// <summary>
    /// 检查通道是否存在
    /// </summary>
    public bool ChannelExists(string channelName)
    {
        return _channels.ContainsKey(channelName);
    }

    /// <summary>
    /// 尝试获取已存在的通道
    /// </summary>
    public bool TryGetChannel<T>(string channelName, out SharedMemoryChannel<T>? channel) where T : struct
    {
        channel = null;
        if (_channels.TryGetValue(channelName, out var existing))
        {
            channel = existing as SharedMemoryChannel<T>;
            return channel != null;
        }
        return false;
    }

    /// <summary>
    /// 移除通道
    /// </summary>
    public bool RemoveChannel(string channelName)
    {
        if (_channels.TryRemove(channelName, out var channel))
        {
            channel.Dispose();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取所有通道名称
    /// </summary>
    public IEnumerable<string> GetChannelNames()
    {
        return _channels.Keys.ToList();
    }

    /// <summary>
    /// 获取通道数量
    /// </summary>
    public int ChannelCount => _channels.Count;

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SharedMemoryChannelManager));
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var channel in _channels.Values)
        {
            channel.Dispose();
        }
        _channels.Clear();

        _disposed = true;
    }
}
