using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using MCCS.Infrastructure.Communication;

namespace MCCS.Infrastructure.Services;

/// <summary>
/// 通道数据绑定器
/// 提供WPF友好的属性通知，可直接用于XAML绑定
/// </summary>
public class ChannelDataBinding : INotifyPropertyChanged, IDisposable
{
    private readonly long _channelId;
    private readonly CompositeDisposable _subscriptions;
    private double _value;
    private double _previousValue;
    private long _sequenceIndex;
    private DateTime _lastUpdateTime;
    private bool _isConnected;

    /// <summary>
    /// 通道ID
    /// </summary>
    public long ChannelId => _channelId;

    /// <summary>
    /// 当前值
    /// </summary>
    public double Value
    {
        get => _value;
        private set
        {
            if (Math.Abs(_value - value) > double.Epsilon)
            {
                _previousValue = _value;
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ValueDelta));
            }
        }
    }

    /// <summary>
    /// 上一个值
    /// </summary>
    public double PreviousValue => _previousValue;

    /// <summary>
    /// 值变化量
    /// </summary>
    public double ValueDelta => _value - _previousValue;

    /// <summary>
    /// 序列索引
    /// </summary>
    public long SequenceIndex
    {
        get => _sequenceIndex;
        private set
        {
            if (_sequenceIndex != value)
            {
                _sequenceIndex = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdateTime
    {
        get => _lastUpdateTime;
        private set
        {
            _lastUpdateTime = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 创建通道数据绑定器
    /// </summary>
    /// <param name="channelId">通道ID</param>
    /// <param name="service">数据服务，如果为null则使用全局服务</param>
    public ChannelDataBinding(long channelId, IChannelDataService? service = null)
    {
        _channelId = channelId;
        _subscriptions = new CompositeDisposable();

        var dataService = service ?? ChannelDataServiceProvider.Instance;

        // 获取初始值
        var currentValue = dataService.GetCurrentValue(channelId);
        if (currentValue.HasValue)
        {
            _value = currentValue.Value;
        }

        // 订阅数据变化
        var dataSubscription = dataService.GetChannelDataStream(channelId)
            .Subscribe(OnDataReceived);
        _subscriptions.Add(dataSubscription);

        // 订阅连接状态
        dataService.ConnectionStateChanged += OnConnectionStateChanged;
        _isConnected = dataService.IsConnected;
    }

    private void OnDataReceived(ChannelDataItem data)
    {
        Value = data.Value;
        SequenceIndex = data.SequenceIndex;
        LastUpdateTime = DateTime.Now;
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        IsConnected = e.IsConnected;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}

/// <summary>
/// 多通道数据绑定器
/// 用于同时绑定多个通道的数据
/// </summary>
public class MultiChannelDataBinding : INotifyPropertyChanged, IDisposable
{
    private readonly Dictionary<long, ChannelDataBinding> _bindings;
    private readonly CompositeDisposable _subscriptions;

    /// <summary>
    /// 获取指定通道的绑定
    /// </summary>
    public ChannelDataBinding? this[long channelId] =>
        _bindings.TryGetValue(channelId, out var binding) ? binding : null;

    /// <summary>
    /// 所有通道ID
    /// </summary>
    public IEnumerable<long> ChannelIds => _bindings.Keys;

    /// <summary>
    /// 所有绑定
    /// </summary>
    public IEnumerable<ChannelDataBinding> Bindings => _bindings.Values;

    /// <summary>
    /// 创建多通道数据绑定器
    /// </summary>
    /// <param name="channelIds">通道ID列表</param>
    /// <param name="service">数据服务，如果为null则使用全局服务</param>
    public MultiChannelDataBinding(IEnumerable<long> channelIds, IChannelDataService? service = null)
    {
        _bindings = new Dictionary<long, ChannelDataBinding>();
        _subscriptions = new CompositeDisposable();

        foreach (var channelId in channelIds)
        {
            var binding = new ChannelDataBinding(channelId, service);
            _bindings[channelId] = binding;
            _subscriptions.Add(binding);

            // 转发属性变化事件
            binding.PropertyChanged += (s, e) =>
            {
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs($"Channel_{channelId}_{e.PropertyName}"));
            };
        }
    }

    /// <summary>
    /// 获取指定通道的当前值
    /// </summary>
    public double? GetValue(long channelId)
    {
        return _bindings.TryGetValue(channelId, out var binding) ? binding.Value : null;
    }

    /// <summary>
    /// 获取所有通道的当前值
    /// </summary>
    public Dictionary<long, double> GetAllValues()
    {
        return _bindings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        _subscriptions.Dispose();
        _bindings.Clear();
    }
}
