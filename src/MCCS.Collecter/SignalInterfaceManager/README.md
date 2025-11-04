# SignalManager 架构设计文档

## 概述

SignalManager 是物理信号接口管理器，专注于管理所有物理信号接口（HardwareSignalChannel），实现**数据采集部分与控制部分的隔离**。

## 核心职责

### SignalManager 负责：
1. **管理物理信号接口** - 添加、移除、查询物理信号
2. **数据采集隔离** - 从硬件设备流中提取每个物理信号的数据
3. **提供数据流接口** - 为虚拟通道和控制通道提供可订阅的数据流

### SignalManager 不负责：
- ❌ 虚拟通道的管理和计算（由外部管理）
- ❌ 控制通道的管理和控制逻辑（由外部管理）
- ❌ 业务逻辑和控制算法

## 架构设计

```
硬件设备 (ControllerHardwareDevice)
    ↓
    ↓ IndividualDataStream (批量数据流)
    ↓
SignalManager (物理信号接口管理)
    ├── HardwareSignalChannel 1 (AI0) → SignalData流
    ├── HardwareSignalChannel 2 (AI1) → SignalData流
    ├── HardwareSignalChannel 3 (SSI0) → SignalData流
    └── ...
         ↓
         ↓ 订阅物理信号数据流
         ↓
外部模块（虚拟通道、控制通道等）
    ├── VirtualChannel - 组合多个物理信号计算
    └── ControlChannel - 使用物理信号进行控制
```

## 核心类说明

### 1. SignalData
信号数据模型，标准化的数据结构：
```csharp
public record SignalData
{
    public long SignalId { get; init; }      // 信号ID
    public double Value { get; init; }       // 数据值
    public long Timestamp { get; init; }     // 时间戳
    public bool IsValid { get; init; }       // 数据是否有效
}
```

### 2. HardwareSignalChannel
物理信号接口，从硬件设备流中提取对应信号的数据：
- **输入**：硬件设备的 `IndividualDataStream`
- **输出**：`IObservable<SignalData>` 数据流
- **功能**：独立管理每个物理信号的数据采集

### 3. ISignalManager / SignalManager
信号接口管理器，统一管理所有物理信号：
- **初始化**：关联硬件设备
- **信号管理**：添加、移除物理信号
- **生命周期**：启动、停止、释放资源
- **数据访问**：提供信号数据流订阅接口

## 使用示例

### 基本使用流程

```csharp
// 1. 创建 SignalManager 实例
var signalManager = new SignalManager();

// 2. 初始化并关联硬件设备
IControllerHardwareDevice device = ...; // 获取硬件设备实例
signalManager.Initialize(device);

// 3. 添加物理信号配置
var signalConfigs = new List<HardwareSignalConfiguration>
{
    new() {
        SignalId = 1,
        SignalName = "力传感器",
        SignalAddress = SignalAddressEnum.AI0,
        DeviceId = 100,
        MinValue = 0,
        MaxValue = 1000,
        Unit = "N"
    },
    new() {
        SignalId = 2,
        SignalName = "位移传感器",
        SignalAddress = SignalAddressEnum.AI1,
        DeviceId = 100,
        MinValue = 0,
        MaxValue = 100,
        Unit = "mm"
    }
};
signalManager.AddPhysicalSignals(signalConfigs);

// 4. 启动信号采集
signalManager.Start();

// 5. 订阅物理信号数据流
var forceDataStream = signalManager.GetSignalDataStream(1);
forceDataStream?.Subscribe(data =>
{
    Console.WriteLine($"力信号: {data.Value} N @ {data.Timestamp}");
});

// 6. 查询信号
var signal = signalManager.GetPhysicalSignal(1);
Console.WriteLine($"信号配置: {signal?.Configuration.SignalName}");

// 7. 停止和释放
signalManager.Stop();
signalManager.Dispose();
```

### 在虚拟通道中使用 SignalManager

虚拟通道可以订阅多个物理信号进行组合计算：

```csharp
// 虚拟通道：计算两个力传感器的平均值
public class AverageForcePseudoChannel
{
    private readonly ISignalManager _signalManager;
    private readonly IDisposable _subscription1;
    private readonly IDisposable _subscription2;

    private double _latestValue1;
    private double _latestValue2;

    public AverageForcePseudoChannel(ISignalManager signalManager)
    {
        _signalManager = signalManager;

        // 订阅物理信号1
        var stream1 = _signalManager.GetSignalDataStream(1);
        _subscription1 = stream1?.Subscribe(data => {
            _latestValue1 = data.Value;
            EmitAverageValue();
        });

        // 订阅物理信号2
        var stream2 = _signalManager.GetSignalDataStream(2);
        _subscription2 = stream2?.Subscribe(data => {
            _latestValue2 = data.Value;
            EmitAverageValue();
        });
    }

    private void EmitAverageValue()
    {
        var average = (_latestValue1 + _latestValue2) / 2.0;
        // 输出虚拟通道的数据
        OnDataAvailable?.Invoke(average);
    }

    public event Action<double>? OnDataAvailable;

    public void Dispose()
    {
        _subscription1?.Dispose();
        _subscription2?.Dispose();
    }
}
```

### 在控制通道中使用 SignalManager

控制通道可以订阅物理信号或虚拟通道作为反馈：

```csharp
// 控制通道：使用力信号进行PID控制
public class ForceControlChannel
{
    private readonly ISignalManager _signalManager;
    private readonly IDisposable _subscription;
    private double _setpoint;

    public ForceControlChannel(ISignalManager signalManager, long feedbackSignalId)
    {
        _signalManager = signalManager;

        // 订阅反馈信号
        var feedbackStream = _signalManager.GetSignalDataStream(feedbackSignalId);
        _subscription = feedbackStream?.Subscribe(data => {
            var feedback = data.Value;
            var output = CalculatePID(feedback);
            SendControlOutput(output);
        });
    }

    public void SetSetpoint(double setpoint)
    {
        _setpoint = setpoint;
    }

    private double CalculatePID(double feedback)
    {
        var error = _setpoint - feedback;
        // PID 控制逻辑
        return error * 0.5; // 简化的比例控制
    }

    private void SendControlOutput(double output)
    {
        // 发送控制指令到硬件设备
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
```

## API 说明

### ISignalManager 接口

| 方法 | 说明 |
|------|------|
| `Initialize(device)` | 初始化并关联硬件设备 |
| `AddPhysicalSignal(config)` | 添加单个物理信号 |
| `AddPhysicalSignals(configs)` | 批量添加物理信号 |
| `RemovePhysicalSignal(signalId)` | 移除物理信号 |
| `Start()` | 启动所有信号采集 |
| `Stop()` | 停止所有信号采集 |
| `GetPhysicalSignal(signalId)` | 获取物理信号接口 |
| `GetAllPhysicalSignals()` | 获取所有物理信号 |
| `GetSignalDataStream(signalId)` | 获取信号数据流（用于订阅） |
| `ContainsSignal(signalId)` | 检查信号是否存在 |
| `IsRunning` | 是否正在运行（属性） |
| `Dispose()` | 释放资源 |

## 设计特点

### ✅ 单一职责
- SignalManager 只管理物理信号接口
- 虚拟通道和控制通道由外部独立管理
- 各模块职责清晰，易于维护

### ✅ 高性能
- 使用 Reactive Extensions (Rx) 实现高性能异步数据流
- `ConcurrentDictionary` 保证线程安全
- 避免不必要的数据拷贝和转换

### ✅ 隔离性
- 物理信号与硬件设备隔离
- 数据采集与控制逻辑隔离
- 各层独立，便于测试和替换

### ✅ 可扩展性
- 接口与实现分离（`ISignalManager`）
- 支持运行时动态添加/移除信号
- 易于添加新的信号类型

### ✅ 简洁性
- API 简单易用
- 代码结构清晰
- 避免过度设计

## 与其他模块的关系

### SignalManager → 虚拟通道（PseudoChannel）
- 虚拟通道订阅 SignalManager 的物理信号数据流
- 虚拟通道组合多个物理信号进行计算
- 虚拟通道输出的数据可供其他模块使用

### SignalManager → 控制通道（ControlChannel）
- 控制通道订阅 SignalManager 的物理信号或虚拟通道作为反馈
- 控制通道实现控制算法（如PID）
- 控制通道输出控制指令到硬件设备

### 数据流向
```
硬件设备 → SignalManager → 物理信号 → 虚拟通道/控制通道 → 业务逻辑
```

## 注意事项

1. **初始化顺序**：必须先调用 `Initialize()` 再调用 `Start()`
2. **线程安全**：所有公共方法都是线程安全的
3. **资源释放**：使用完后必须调用 `Dispose()` 释放资源
4. **信号ID唯一性**：每个信号的ID必须唯一
5. **硬件设备类型**：设备必须继承自 `ControllerHardwareDeviceBase`

## 后续扩展建议

当前版本已实现核心功能，以下是可选的扩展方向：

- [ ] 信号数据质量监控
- [ ] 信号数据的历史缓冲
- [ ] 信号采样率动态调整
- [ ] 信号数据的统计分析（最大值、最小值、平均值等）
- [ ] 信号数据的持久化存储
