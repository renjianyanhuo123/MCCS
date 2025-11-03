# SignalManager 架构设计文档

## 概述

SignalManager 实现了数据采集和控制部分的隔离，通过物理信号接口实现底层硬件设备与上层控制逻辑的解耦。

## 架构层次

### 1. 物理信号层 (HardwareSignalChannel)
- **职责**：从硬件设备的数据流中提取对应的物理信号数据
- **输入**：硬件设备的 `IndividualDataStream`
- **输出**：单个物理信号的数据流 `IObservable<SignalData>`
- **特点**：每个物理信号独立管理，支持热插拔

### 2. 虚拟通道层 (VirtualChannel)
- **职责**：组合多个物理信号进行数学运算，输出计算结果
- **输入**：多个物理信号的数据流
- **配置**：支持自定义计算公式（如：`{1} + {2} * 0.5`）
- **输出**：虚拟通道的数据流 `IObservable<SignalData>`
- **特点**：支持动态添加/删除，实时计算

### 3. 控制通道层 (ControlChannel)
- **职责**：实现闭环控制逻辑
- **输入**：反馈信号（位置/力）+ 设定值
- **输出**：控制指令发送到硬件设备
- **支持**：可订阅物理信号或虚拟通道作为反馈
- **特点**：支持多种控制模式（开环/闭环）

### 4. 信号管理器 (SignalManager)
- **职责**：统一管理所有信号和通道
- **功能**：
  - 初始化和生命周期管理
  - 动态添加/删除虚拟通道和控制通道
  - 提供统一的查询接口

## 使用示例

### 基本使用流程

```csharp
// 1. 创建SignalManager实例
var signalManager = new SignalManager();

// 2. 初始化物理信号配置
var signalConfigs = new List<HardwareSignalConfiguration>
{
    new() { SignalId = 1, SignalName = "AI0", SignalAddress = SignalAddressEnum.AI0, DeviceId = 100 },
    new() { SignalId = 2, SignalName = "AI1", SignalAddress = SignalAddressEnum.AI1, DeviceId = 100 }
};
signalManager.InitializePhysicalSignals(signalConfigs);

// 3. 关联硬件设备
IControllerHardwareDevice device = ...; // 获取硬件设备实例
signalManager.Initialize(device);

// 4. 添加虚拟通道
signalManager.AddVirtualChannel(
    channelId: 1001,
    channelName: "平均力",
    formula: "({1} + {2}) / 2",  // 信号1和信号2的平均值
    signalIds: new List<long> { 1, 2 },
    rangeMin: 0,
    rangeMax: 1000
);

// 5. 添加控制通道
signalManager.AddControlChannel(
    channelId: 2001,
    channelName: "力控制通道",
    channelType: ChannelTypeEnum.Force,
    controlMode: ControlChannelModeTypeEnum.FCSLoop,
    controlCycle: 20.0,  // 20ms控制周期
    positionSignalId: null,
    forceSignalId: 1001,  // 使用虚拟通道作为反馈
    outputSignalId: 10,
    outputLimitation: 80
);

// 6. 启动所有信号和通道
signalManager.Start();

// 7. 订阅信号数据流
var dataStream = signalManager.GetSignalDataStream(1001);
dataStream?.Subscribe(data =>
{
    Console.WriteLine($"虚拟通道数据: {data.Value} @ {data.Timestamp}");
});

// 8. 获取控制通道并设置目标值
var controlChannel = signalManager.GetControlChannel(2001);
controlChannel?.SetSetpoint(500.0);

// 9. 停止和释放
signalManager.Stop();
signalManager.Dispose();
```

### 虚拟通道公式支持

虚拟通道支持以下公式格式：
- `{SignalId}` 或 `[SignalId]` - 引用信号值
- 支持基本数学运算：`+`, `-`, `*`, `/`, `(`, `)`
- 示例：
  - `{1} * 2 + {2}` - 信号1的2倍加上信号2
  - `({1} + {2}) / 2` - 信号1和2的平均值
  - `{1} * 0.5 + 100` - 线性变换

## 设计特点

### 高性能
- 使用 Reactive Extensions (Rx) 实现异步数据流
- 采用 `ConcurrentDictionary` 保证线程安全
- 避免不必要的数据拷贝

### 可扩展性
- 接口与实现分离（`ISignalManager`）
- 支持动态添加/删除通道
- 易于添加新的信号类型和控制算法

### 隔离性
- 物理信号层与硬件设备隔离
- 虚拟通道独立于物理信号
- 控制通道可选择物理或虚拟信号作为反馈

### 简洁性
- 核心类职责清晰
- API 简单易用
- 避免过度设计

## 注意事项

1. **线程安全**：所有公共方法都是线程安全的
2. **生命周期**：必须在使用完后调用 `Dispose()` 释放资源
3. **初始化顺序**：必须先调用 `Initialize()` 再调用 `Start()`
4. **公式验证**：虚拟通道公式需确保引用的信号存在
5. **性能考虑**：虚拟通道的计算频率取决于输入信号的采样率

## 后续扩展

可以考虑的扩展功能（当前版本未实现）：
- 信号数据质量监控和异常检测
- 控制通道的完整PID算法实现
- 信号数据的历史记录和回放
- 更复杂的公式表达式支持（三角函数、指数等）
- 控制通道的自动调优功能
