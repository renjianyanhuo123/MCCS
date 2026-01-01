# 工作流序列化模块使用说明

## 概述

本模块提供了完整的工作流序列化和反序列化功能，支持将工作流保存为JSON格式并从JSON恢复工作流。

**✅ 重要特性**：从JSON恢复的工作流**完全可操作**，与手动创建的工作流功能完全一致：
- ✅ 可以正常添加节点
- ✅ 可以正常删除节点
- ✅ 可以操作决策节点（折叠、展开、添加分支、删除分支）
- ✅ 所有命令和事件系统正常工作
- ✅ UI交互完全正常

## 架构设计

### 核心组件

1. **DTO层** (`Dtos/`目录)
   - `WorkflowDto`: 工作流数据传输对象
   - `NodeDto`: 节点数据传输对象
   - `ConnectionDto`: 连接线数据传输对象
   - `PointDto`: 坐标点数据传输对象

2. **序列化服务**
   - `IWorkflowSerializer`: 序列化服务接口
   - `WorkflowSerializer`: 序列化服务实现

3. **画布管理器**
   - `ICanvasManager`: 画布管理接口
   - `CanvasManager`: 画布管理实现

### 设计原则

- **高内聚低耦合**: 序列化逻辑独立于UI和业务逻辑
- **关注点分离**: DTO与领域模型分离，避免序列化污染领域模型
- **依赖注入**: 通过Prism容器管理服务生命周期
- **避免循环引用**: DTO不包含Parent引用，通过递归结构表示层级关系

## 使用方法

### 1. 服务注册

服务已在`WorkflowModule.cs`中注册：

```csharp
// 序列化服务（单例）
containerRegistry.RegisterSingleton<IWorkflowSerializer, WorkflowSerializer>();

// 画布管理器（单例）
containerRegistry.RegisterSingleton<ICanvasManager, CanvasManager>();
```

### 2. 保存工作流

#### 方式1: 保存为JSON字符串

```csharp
// 注入依赖
private readonly ICanvasManager _canvasManager;

public MyViewModel(ICanvasManager canvasManager)
{
    _canvasManager = canvasManager;
}

// 设置当前工作流根节点
_canvasManager.SetWorkflowRoot(myStepListNodes);

// 保存为JSON字符串
string json = _canvasManager.SaveWorkflowToJson("我的工作流");
```

#### 方式2: 保存到文件

```csharp
// 保存到文件（异步）
await _canvasManager.SaveWorkflowToFileAsync(
    @"C:\workflows\my_workflow.json",
    "我的工作流"
);
```

#### 方式3: 直接使用序列化器

```csharp
private readonly IWorkflowSerializer _serializer;

public MyViewModel(IWorkflowSerializer serializer)
{
    _serializer = serializer;
}

// 序列化为JSON字符串
string json = _serializer.SerializeToJson(rootNode, "工作流名称");

// 或获取DTO对象
WorkflowDto dto = _serializer.SerializeToDto(rootNode, "工作流名称");
```

### 3. 加载工作流

#### 方式1: 从JSON字符串加载

```csharp
// 渲染工作流（会自动设置到画布管理器）
_canvasManager.RenderWorkflowByJson(jsonString);

// 获取加载的根节点
StepListNodes? rootNode = _canvasManager.GetWorkflowRoot();
```

#### 方式2: 从文件加载

```csharp
// 从文件加载（异步）
await _canvasManager.LoadWorkflowFromFileAsync(@"C:\workflows\my_workflow.json");

// 获取加载的根节点
StepListNodes? rootNode = _canvasManager.GetWorkflowRoot();
```

#### 方式3: 直接使用序列化器

```csharp
// 从JSON反序列化
StepListNodes rootNode = _serializer.DeserializeFromJson(
    jsonString,
    eventAggregator,
    dialogService // 可选
);
```

### 4. 示例：完整的保存和加载流程

```csharp
public class WorkflowEditorViewModel : BindableBase
{
    private readonly ICanvasManager _canvasManager;
    private readonly IDialogService _dialogService;

    public WorkflowEditorViewModel(
        ICanvasManager canvasManager,
        IDialogService dialogService)
    {
        _canvasManager = canvasManager;
        _dialogService = dialogService;

        SaveCommand = new AsyncDelegateCommand(OnSaveAsync);
        LoadCommand = new AsyncDelegateCommand(OnLoadAsync);
    }

    public AsyncDelegateCommand SaveCommand { get; }
    public AsyncDelegateCommand LoadCommand { get; }

    private async Task OnSaveAsync()
    {
        try
        {
            // 弹出保存文件对话框
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "工作流文件 (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "workflow"
            };

            if (dialog.ShowDialog() == true)
            {
                await _canvasManager.SaveWorkflowToFileAsync(
                    dialog.FileName,
                    "我的工作流"
                );

                // 显示成功消息
                await _dialogService.ShowDialogHostAsync(
                    "MessageDialog",
                    "RootDialog",
                    new DialogParameters { { "Message", "保存成功！" } }
                );
            }
        }
        catch (Exception ex)
        {
            // 错误处理
            await _dialogService.ShowDialogHostAsync(
                "MessageDialog",
                "RootDialog",
                new DialogParameters { { "Message", $"保存失败: {ex.Message}" } }
            );
        }
    }

    private async Task OnLoadAsync()
    {
        try
        {
            // 弹出打开文件对话框
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "工作流文件 (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                await _canvasManager.LoadWorkflowFromFileAsync(dialog.FileName);

                // 获取加载的工作流
                var rootNode = _canvasManager.GetWorkflowRoot();

                // 更新UI绑定
                MyWorkflow = rootNode;

                // 显示成功消息
                await _dialogService.ShowDialogHostAsync(
                    "MessageDialog",
                    "RootDialog",
                    new DialogParameters { { "Message", "加载成功！" } }
                );
            }
        }
        catch (Exception ex)
        {
            // 错误处理
            await _dialogService.ShowDialogHostAsync(
                "MessageDialog",
                "RootDialog",
                new DialogParameters { { "Message", $"加载失败: {ex.Message}" } }
            );
        }
    }

    private StepListNodes? _myWorkflow;
    public StepListNodes? MyWorkflow
    {
        get => _myWorkflow;
        set => SetProperty(ref _myWorkflow, value);
    }
}
```

## JSON格式说明

### 示例JSON结构

```json
{
  "id": "abc123",
  "name": "我的工作流",
  "version": "1.0.0",
  "createdAt": "2024-01-01T00:00:00",
  "updatedAt": "2024-01-01T00:00:00",
  "rootNode": {
    "id": "node001",
    "type": "StepList",
    "name": "",
    "code": "",
    "index": 0,
    "order": 0,
    "level": 0,
    "positionX": 0,
    "positionY": 0,
    "width": 300,
    "height": 600,
    "children": [
      {
        "id": "node002",
        "type": "Start",
        "name": "Start",
        "width": 60,
        "height": 60,
        "properties": {}
      },
      {
        "id": "node003",
        "type": "Process",
        "name": "",
        "width": 260,
        "height": 85,
        "properties": {
          "title": "步骤1",
          "titleBackgroundColor": "#FFFF9955"
        }
      },
      {
        "id": "node004",
        "type": "Decision",
        "name": "",
        "width": 580,
        "height": 200,
        "properties": {
          "isCollapse": true,
          "decisionNum": 2,
          "itemSpacing": 30,
          "borderWidth": 400,
          "borderHeight": 200
        },
        "children": [
          {
            "id": "branch001",
            "type": "BranchStepList",
            "width": 260,
            "height": 200,
            "children": [...]
          },
          {
            "id": "branch002",
            "type": "BranchStepList",
            "width": 260,
            "height": 200,
            "children": [...]
          }
        ]
      },
      {
        "id": "node005",
        "type": "End",
        "name": "End",
        "width": 56,
        "height": 80,
        "properties": {}
      }
    ],
    "properties": {
      "addActionDistance": 35
    }
  },
  "connections": [
    {
      "id": "conn001",
      "type": "Sequential",
      "sourceNodeId": "",
      "targetNodeId": "",
      "points": [
        { "x": 150, "y": 60 },
        { "x": 150, "y": 95 }
      ],
      "properties": {}
    }
  ],
  "metadata": {}
}
```

## 节点类型映射

| 节点类型枚举 | 描述 | 特殊属性 |
|------------|------|---------|
| `Start` | 开始节点 | 无 |
| `End` | 结束节点 | 无 |
| `Process` | 步骤节点 | `title`, `titleBackgroundColor` |
| `Decision` | 决策节点 | `isCollapse`, `decisionNum`, `itemSpacing`, `borderWidth`, `borderHeight` |
| `Branch` | 分支节点 | `title` |
| `StepList` | 主流程容器 | `addActionDistance` |
| `BranchStepList` | 分支流程容器 | `addActionDistance` |

## 注意事项

### 1. 不序列化的属性

以下属性不会被序列化：
- `Parent`: 避免循环引用
- `Command`: 不可序列化的委托对象
- `IEventAggregator`: 依赖注入的运行时对象
- `IDialogService`: 依赖注入的运行时对象
- 临时UI状态（如`IsOpen`, `IsShowShade`）

### 2. 节点重建

反序列化时会：
- 重新创建所有节点实例
- 重新注入依赖（EventAggregator、DialogService）
- 重新建立Parent-Child关系
- 自动添加AddOpNode（添加操作节点）
- 触发RenderChanged()更新布局

### 3. ID管理

- 默认情况下，反序列化会生成新的节点ID
- 如需保持原ID，可在`RestoreBaseNodeProperties`方法中取消注释`node.Id = nodeDto.Id;`

### 4. 扩展属性

使用`Properties`字典存储节点特定属性：
- 易于扩展，不影响DTO结构
- 支持动态类型（通过`object`类型）
- 反序列化时需要类型转换

## 扩展开发

### 添加新节点类型

1. **定义节点枚举**（`NodeTypeEnum.cs`）
```csharp
public enum NodeTypeEnum
{
    // ... 现有类型
    CustomNode  // 新类型
}
```

2. **创建节点类**
```csharp
public class CustomNode : BaseNode
{
    public string CustomProperty { get; set; }
}
```

3. **序列化器中添加处理**（`WorkflowSerializer.cs`）
```csharp
private NodeDto ConvertNodeToDto(BaseNode node)
{
    // ... 现有代码

    switch (node)
    {
        // ... 现有case
        case CustomNode customNode:
            SerializeCustomNode(customNode, nodeDto);
            break;
    }
}

private void SerializeCustomNode(CustomNode customNode, NodeDto nodeDto)
{
    nodeDto.Properties["CustomProperty"] = customNode.CustomProperty;
}
```

4. **反序列化器中添加处理**
```csharp
private BaseNode? CreateNodeFromDto(NodeDto nodeDto, ...)
{
    BaseNode? node = nodeDto.Type switch
    {
        // ... 现有case
        NodeTypeEnum.CustomNode => new CustomNode(eventAggregator),
        _ => null
    };
}

private void RestoreSpecificNodeProperties(NodeDto nodeDto, BaseNode node, ...)
{
    switch (node)
    {
        // ... 现有case
        case CustomNode customNode:
            if (nodeDto.Properties.TryGetValue("CustomProperty", out var prop))
                customNode.CustomProperty = prop?.ToString() ?? string.Empty;
            break;
    }
}
```

### 自定义序列化格式

如需支持XML或其他格式：

1. **创建新的序列化器**
```csharp
public class XmlWorkflowSerializer : IWorkflowSerializer
{
    // 实现接口方法，使用XML序列化
}
```

2. **注册到容器**
```csharp
containerRegistry.Register<IWorkflowSerializer, XmlWorkflowSerializer>("xml");
```

## 测试建议

### 单元测试

```csharp
[Fact]
public void SerializeAndDeserialize_ShouldPreserveWorkflowStructure()
{
    // Arrange
    var serializer = new WorkflowSerializer();
    var eventAggregator = new EventAggregator();
    var rootNode = CreateTestWorkflow(eventAggregator);

    // Act
    var json = serializer.SerializeToJson(rootNode);
    var restored = serializer.DeserializeFromJson(json, eventAggregator);

    // Assert
    Assert.Equal(rootNode.Nodes.Count - 1, restored.Nodes.Count - 1); // -1排除AddOpNode
    Assert.Equal(rootNode.Width, restored.Width);
    Assert.Equal(rootNode.Height, restored.Height);
}
```

### 集成测试

1. 创建复杂工作流（包含决策节点、多层嵌套）
2. 保存到文件
3. 清空当前工作流
4. 从文件加载
5. 验证UI渲染是否正确
6. 验证所有交互功能是否正常

## 性能优化

### 大型工作流

对于节点数超过1000的大型工作流：

1. **异步序列化**
```csharp
public async Task<string> SerializeToJsonAsync(StepListNodes rootNode)
{
    return await Task.Run(() => SerializeToJson(rootNode));
}
```

2. **增量保存**
只序列化变更的节点，而非整个工作流

3. **压缩JSON**
```csharp
var options = new JsonSerializerOptions
{
    WriteIndented = false  // 减小文件大小
};
```

## 许可证

本模块是MCCS项目的一部分，遵循项目整体许可证。

## 贡献

如有问题或改进建议，请通过项目Issue提交。
