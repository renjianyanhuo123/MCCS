# SignalManager 架构设计文档

## 概述

SignalManager 是物理信号接口管理器，负责管理**多个硬件控制器及其物理信号接口**，实现数据采集部分与控制部分的隔离。

## 核心职责

### SignalManager 负责：
1. **管理多个硬件控制器** - 添加、移除、查询控制器设备
2. **管理物理信号接口** - 添加、移除、查询所有控制器的物理信号
3. **数据采集隔离** - 从各个控制器的数据流中提取对应物理信号的数据
4. **提供数据流接口** - 为虚拟通道和控制通道提供可订阅的数据流

### SignalManager 不负责：
- ❌ 虚拟通道的管理和计算（由外部管理）
- ❌ 控制通道的管理和控制逻辑（由外部管理）
- ❌ 业务逻辑和控制算法

## 架构设计

```
SignalManager
    ↓
    ├── 控制器 1 (Device 100)
    │   ├── IndividualDataStream (批量数据流)
    │   ├── HardwareSignalChannel 1 (AI0) → SignalData流
    │   ├── HardwareSignalChannel 2 (AI1) → SignalData流
    │   └── HardwareSignalChannel 3 (SSI0) → SignalData流
    │
    ├── 控制器 2 (Device 200)
    │   ├── IndividualDataStream (批量数据流)
    │   ├── HardwareSignalChannel 4 (AI0) → SignalData流
    │   └── HardwareSignalChannel 5 (AI1) → SignalData流
    │
    └── 控制器 3 (Device 300)
        ├── IndividualDataStream (批量数据流)
        └── HardwareSignalChannel 6 (AI0) → SignalData流
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
- **输入**：所属控制器的 `IndividualDataStream`
- **输出**：`IObservable<SignalData>` 数据流
- **关联**：通过 `DeviceId` 关联到所属的控制器
- **特性**：使用 Rx 操作符直接派生，不使用 Subject 转发

### 3. ISignalManager / SignalManager
信号接口管理器，统一管理所有控制器和物理信号：
- **控制器管理**：添加、移除、查询控制器设备
- **信号管理**：添加、移除、查询物理信号
- **自动关联**：信号通过 DeviceId 自动关联到对应控制器
- **生命周期**：启动、停止、释放资源
- **数据访问**：提供信号数据流订阅接口

## 使用示例

### 基本使用流程

```csharp
// 1. 创建 SignalManager 实例
var signalManager = new SignalManager();

// 2. 添加多个硬件控制器设备
IControllerHardwareDevice device1 = ...; // 控制器1, DeviceId = 100
IControllerHardwareDevice device2 = ...; // 控制器2, DeviceId = 200
IControllerHardwareDevice device3 = ...; // 控制器3, DeviceId = 300

signalManager.AddDevice(device1);
signalManager.AddDevice(device2);
signalManager.AddDevice(device3);

// 3. 添加各个控制器的物理信号配置
var signalConfigs = new List<HardwareSignalConfiguration>
{
    // 控制器1的信号
    new() {
        SignalId = 1,
        SignalName = "控制器1-力传感器",
        SignalAddress = SignalAddressEnum.AI0,
        DeviceId = 100,  // 关联到控制器1
        MinValue = 0,
        MaxValue = 1000,
        Unit = "N"
    },
    new() {
        SignalId = 2,
        SignalName = "控制器1-位移传感器",
        SignalAddress = SignalAddressEnum.AI1,
        DeviceId = 100,  // 关联到控制器1
        MinValue = 0,
        MaxValue = 100,
        Unit = "mm"
    },

    // 控制器2的信号
    new() {
        SignalId = 3,
        SignalName = "控制器2-力传感器",
        SignalAddress = SignalAddressEnum.AI0,
        DeviceId = 200,  // 关联到控制器2
        MinValue = 0,
        MaxValue = 2000,
        Unit = "N"
    },

    // 控制器3的信号
    new() {
        SignalId = 4,
        SignalName = "控制器3-温度传感器",
        SignalAddress = SignalAddressEnum.AI0,
        DeviceId = 300,  // 关联到控制器3
        MinValue = -50,
        MaxValue = 150,
        Unit = "°C"
    }
};
signalManager.AddPhysicalSignals(signalConfigs);

// 4. 启动所有信号采集（初始化所有信号流）
signalManager.Start();

// 5. 订阅物理信号数据流
var forceDataStream1 = signalManager.GetSignalDataStream(1); // 控制器1的力信号
forceDataStream1?.Subscribe(data =>
{
    Console.WriteLine($"控制器1力信号: {data.Value} N @ {data.Timestamp}");
});

var forceDataStream2 = signalManager.GetSignalDataStream(3); // 控制器2的力信号
forceDataStream2?.Subscribe(data =>
{
    Console.WriteLine($"控制器2力信号: {data.Value} N @ {data.Timestamp}");
});

// 6. 查询特定控制器的所有信号
var device1Signals = signalManager.GetPhysicalSignalsByDevice(100);
Console.WriteLine($"控制器1有 {device1Signals.Count} 个信号");

// 7. 运行时动态添加新控制器和信号
IControllerHardwareDevice device4 = ...; // 新的控制器4
signalManager.AddDevice(device4);
signalManager.AddPhysicalSignal(new HardwareSignalConfiguration
{
    SignalId = 5,
    SignalName = "控制器4-压力传感器",
    SignalAddress = SignalAddressEnum.AI0,
    DeviceId = 400,
    Unit = "Pa"
});

// 8. 移除控制器（会自动移除该控制器的所有信号）
signalManager.RemoveDevice(300);

// 9. 停止和释放
signalManager.Stop();
signalManager.Dispose();
```

### 在虚拟通道中跨控制器组合信号

虚拟通道可以订阅来自不同控制器的物理信号进行组合计算：

```csharp
// 虚拟通道：计算两个控制器的力传感器的平均值
public class AverageForcePseudoChannel
{
    private readonly ISignalManager _signalManager;
    private readonly IDisposable _subscription1;
    private readonly IDisposable _subscription2;

    private double _latestForce1; // 控制器1的力值
    private double _latestForce2; // 控制器2的力值

    public AverageForcePseudoChannel(ISignalManager signalManager)
    {
        _signalManager = signalManager;

        // 订阅控制器1的力信号
        var stream1 = _signalManager.GetSignalDataStream(1);
        _subscription1 = stream1?.Subscribe(data => {
            _latestForce1 = data.Value;
            EmitAverageValue();
        });

        // 订阅控制器2的力信号
        var stream2 = _signalManager.GetSignalDataStream(3);
        _subscription2 = stream2?.Subscribe(data => {
            _latestForce2 = data.Value;
            EmitAverageValue();
        });
    }

    private void EmitAverageValue()
    {
        var average = (_latestForce1 + _latestForce2) / 2.0;
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

### 按控制器管理信号

```csharp
// 获取所有控制器
var devices = signalManager.GetAllDevices();
foreach (var device in devices)
{
    Console.WriteLine($"控制器: {device.DeviceName} (ID: {device.DeviceId})");

    // 获取该控制器的所有信号
    var signals = signalManager.GetPhysicalSignalsByDevice(device.DeviceId);
    foreach (var signal in signals)
    {
        Console.WriteLine($"  - 信号: {signal.Configuration.SignalName}");
    }
}

// 移除特定控制器及其所有信号
if (signalManager.ContainsDevice(200))
{
    signalManager.RemoveDevice(200); // 自动移除控制器2的所有信号
}
```

## API 说明

### ISignalManager 接口

#### 控制器管理

| 方法 | 说明 |
|------|------|
| `AddDevice(device)` | 添加硬件控制器设备 |
| `RemoveDevice(deviceId)` | 移除控制器及其所有信号 |
| `GetDevice(deviceId)` | 获取指定控制器 |
| `GetAllDevices()` | 获取所有控制器 |
| `ContainsDevice(deviceId)` | 检查控制器是否存在 |

#### 信号管理

| 方法 | 说明 |
|------|------|
| `AddPhysicalSignal(config)` | 添加单个物理信号（需指定DeviceId） |
| `AddPhysicalSignals(configs)` | 批量添加物理信号 |
| `RemovePhysicalSignal(signalId)` | 移除物理信号 |
| `GetPhysicalSignal(signalId)` | 获取物理信号接口 |
| `GetAllPhysicalSignals()` | 获取所有物理信号 |
| `GetPhysicalSignalsByDevice(deviceId)` | 获取指定控制器的所有信号 |
| `ContainsSignal(signalId)` | 检查信号是否存在 |

#### 数据流和生命周期

| 方法/属性 | 说明 |
|----------|------|
| `Start()` | 启动所有信号采集 |
| `Stop()` | 停止所有信号采集 |
| `GetSignalDataStream(signalId)` | 获取信号数据流（用于订阅） |
| `IsRunning` | 是否正在运行（属性） |
| `Dispose()` | 释放资源 |

## 设计特点

### ✅ 多控制器支持
- 统一管理多个硬件控制器
- 信号通过 DeviceId 自动关联到对应控制器
- 移除控制器时自动清理其所有信号

### ✅ 单一职责
- SignalManager 专注于管理控制器和物理信号
- 虚拟通道和控制通道由外部独立管理
- 各模块职责清晰，易于维护

### ✅ 高性能
- 使用 Reactive Extensions (Rx) 实现高性能异步数据流
- Rx 操作符直接派生，不使用 Subject 转发
- `Publish().RefCount()` 实现热流共享和自动订阅管理
- `ConcurrentDictionary` 保证线程安全

### ✅ 隔离性
- 物理信号与硬件设备隔离
- 数据采集与控制逻辑隔离
- 各层独立，便于测试和替换

### ✅ 可扩展性
- 接口与实现分离（`ISignalManager`）
- 支持运行时动态添加/移除控制器和信号
- 易于添加新的控制器类型和信号类型

### ✅ 简洁性
- API 简单易用
- 代码结构清晰
- 避免过度设计

## 数据流工作原理

```
控制器1.IndividualDataStream (源流)
    ↓
    ↓ Select (提取信号1数据)
    ↓ Where (过滤 null)
    ↓ Publish + RefCount (热流 + 自动订阅管理)
    ↓
Signal 1.DataStream → 外部订阅者

控制器2.IndividualDataStream (源流)
    ↓
    ↓ Select (提取信号3数据)
    ↓ Where (过滤 null)
    ↓ Publish + RefCount
    ↓
Signal 3.DataStream → 外部订阅者
```

每个信号的数据流都是从其所属控制器的数据流直接派生，互不干扰。

## 注意事项

1. **添加顺序**：必须先添加控制器，再添加该控制器的信号
2. **DeviceId必填**：每个信号必须指定 DeviceId 关联到对应控制器
3. **线程安全**：所有公共方法都是线程安全的
4. **资源释放**：使用完后必须调用 `Dispose()` 释放资源
5. **信号ID唯一性**：所有信号的ID必须全局唯一（即使在不同控制器上）
6. **控制器类型**：设备必须继承自 `ControllerHardwareDeviceBase`
7. **级联删除**：移除控制器会自动移除其所有信号

## 后续扩展建议

当前版本已实现核心功能，以下是可选的扩展方向：

- [ ] 信号数据质量监控
- [ ] 信号数据的历史缓冲
- [ ] 信号采样率动态调整
- [ ] 信号数据的统计分析（最大值、最小值、平均值等）
- [ ] 控制器健康状态监控
- [ ] 信号数据的持久化存储
- [ ] 支持信号别名和分组管理
