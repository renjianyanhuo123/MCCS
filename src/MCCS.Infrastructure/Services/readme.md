方式1：使用全局服务提供者
// 初始化（通常在应用启动时调用一次）
await ChannelDataServiceProvider.EnsureStartedAsync();

// 在任意项目中获取数据
var value = ChannelDataServiceProvider.Instance.GetCurrentValue(channelId);

// 订阅数据流
var subscription = ChannelDataServiceProvider.Instance
    .GetChannelDataStream(channelId)
    .Subscribe(data => Console.WriteLine($"Value: {data.Value}"));

方式2：在WPF中使用数据绑定
// ViewModel中
public ChannelDataBinding Channel1 { get; }

public MyViewModel()
{
    Channel1 = new ChannelDataBinding(channelId: 1);
}

// XAML中绑定
// <TextBlock Text="{Binding Channel1.Value}" />

方式三: 使用扩展方法进行流处理
service.GetChannelDataStream(channelId)
    .Sample(TimeSpan.FromMilliseconds(200))  // 采样
    .DistinctUntilValueChanged(tolerance: 0.01)  // 去重
    .Subscribe(data => UpdateUI(data.Value));

