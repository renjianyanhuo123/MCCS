# MCCS.WorkflowSetting 架构设计文档

## 架构原则

### 关注点分离（Separation of Concerns）

本模块严格遵循MVVM架构模式，各层职责明确：

```
┌─────────────────────────────────────────────────────────────┐
│                         View Layer (XAML)                    │
│  - WorkflowStepListNodes.xaml                               │
│  - WorkflowDecisionNode.xaml                                │
│  - 其他UI组件                                                │
│  职责：UI渲染、用户交互                                        │
└──────────────────────┬──────────────────────────────────────┘
                       │ DataBinding
┌──────────────────────▼──────────────────────────────────────┐
│                    ViewModel Layer                           │
│  - StepListNodes (既是Model又是ViewModel)                   │
│  - DecisionNode                                             │
│  - 其他节点类 (继承BindableBase)                             │
│  职责：数据绑定、命令处理、属性通知                            │
└──────────────────────┬──────────────────────────────────────┘
                       │ 使用
┌──────────────────────▼──────────────────────────────────────┐
│                    Service Layer                             │
│  - ICanvasManager / CanvasManager                           │
│  - IWorkflowSerializer / WorkflowSerializer                 │
│  职责：业务逻辑、数据序列化、状态管理                          │
│  ❌ 不涉及UI操作                                             │
└──────────────────────┬──────────────────────────────────────┘
                       │ 操作
┌──────────────────────▼──────────────────────────────────────┐
│                      Data Layer                              │
│  - WorkflowDto                                              │
│  - NodeDto                                                  │
│  - JSON文件                                                 │
│  职责：数据持久化                                             │
└─────────────────────────────────────────────────────────────┘
```

## CanvasManager 设计说明

### ❌ 为什么不需要Canvas参数？

#### 错误的设计（已移除）

```csharp
public interface ICanvasManager
{
    void Inititial(Canvas canvas);  // ❌ 错误：业务层不应持有UI引用
    void RenderWorkflowByJson(string json);  // ❌ 错误：方法名暗示UI操作
}

public class CanvasManager : ICanvasManager
{
    private Canvas? _canvas;  // ❌ 错误：业务层持有UI控件引用

    public void RenderWorkflowByJson(string json)
    {
        var workflow = Deserialize(json);
        _canvas.DataContext = workflow;  // ❌ 错误：业务层操作UI
    }
}
```

**问题分析：**
1. **违反单一职责**：CanvasManager既管理数据又操作UI
2. **违反依赖倒置**：高层业务逻辑依赖底层UI控件
3. **无法单元测试**：依赖WPF Canvas，需要UI线程
4. **违反MVVM**：业务层直接设置View的DataContext

#### ✅ 正确的设计（当前实现）

```csharp
public interface ICanvasManager
{
    // ✅ 只负责数据操作
    StepListNodes LoadWorkflowFromJson(string json);
    Task<StepListNodes> LoadWorkflowFromFileAsync(string filePath);
    string SaveWorkflowToJson(string workflowName = "");
    Task SaveWorkflowToFileAsync(string filePath, string workflowName = "");
    void SetWorkflowRoot(StepListNodes rootNode);
    StepListNodes? GetWorkflowRoot();
}

public class CanvasManager : ICanvasManager
{
    // ✅ 只持有业务数据
    private StepListNodes? _currentWorkflowRoot;

    public StepListNodes LoadWorkflowFromJson(string json)
    {
        // ✅ 只负责反序列化，返回数据
        var workflow = _serializer.DeserializeFromJson(json, ...);
        _currentWorkflowRoot = workflow;
        return workflow;  // 由调用者负责UI绑定
    }
}
```

**优势：**
1. ✅ **单一职责**：只负责数据管理
2. ✅ **依赖倒置**：不依赖UI控件
3. ✅ **可测试性**：可在单元测试中使用
4. ✅ **符合MVVM**：UI绑定由ViewModel/View负责

## 正确的使用方式

### 场景1：在ViewModel中加载工作流

```csharp
public class WorkflowEditorViewModel : BindableBase
{
    private readonly ICanvasManager _canvasManager;

    // 绑定到View的工作流属性
    private StepListNodes? _workflow;
    public StepListNodes? Workflow
    {
        get => _workflow;
        set => SetProperty(ref _workflow, value);
    }

    // 构造函数（依赖注入）
    public WorkflowEditorViewModel(ICanvasManager canvasManager)
    {
        _canvasManager = canvasManager;
        LoadCommand = new AsyncDelegateCommand<string>(LoadWorkflowAsync);
        SaveCommand = new AsyncDelegateCommand<string>(SaveWorkflowAsync);
    }

    // 命令
    public AsyncDelegateCommand<string> LoadCommand { get; }
    public AsyncDelegateCommand<string> SaveCommand { get; }

    // 加载工作流
    private async Task LoadWorkflowAsync(string filePath)
    {
        try
        {
            // ✅ CanvasManager只负责加载数据
            var workflow = await _canvasManager.LoadWorkflowFromFileAsync(filePath);

            // ✅ ViewModel负责将数据绑定到UI
            Workflow = workflow;  // 属性变更通知会自动触发UI更新
        }
        catch (Exception ex)
        {
            // 错误处理
            ShowError($"加载失败: {ex.Message}");
        }
    }

    // 保存工作流
    private async Task SaveWorkflowAsync(string filePath)
    {
        try
        {
            // ✅ 先设置当前工作流
            _canvasManager.SetWorkflowRoot(Workflow);

            // ✅ 保存到文件
            await _canvasManager.SaveWorkflowToFileAsync(filePath, "我的工作流");

            ShowSuccess("保存成功！");
        }
        catch (Exception ex)
        {
            ShowError($"保存失败: {ex.Message}");
        }
    }
}
```

### 场景2：在XAML中绑定

```xml
<Window xmlns:local="clr-namespace:MCCS.WorkflowSetting.Components"
        DataContext="{Binding WorkflowEditorViewModel}">

    <!-- 工具栏 -->
    <StackPanel Orientation="Horizontal">
        <Button Content="打开" Command="{Binding LoadCommand}" />
        <Button Content="保存" Command="{Binding SaveCommand}" />
    </StackPanel>

    <!-- 工作流画布 -->
    <!-- ✅ 通过DataBinding自动渲染 -->
    <local:WorkflowStepListNodes DataContext="{Binding Workflow}" />
</Window>
```

### 场景3：在Code-Behind中使用（不推荐，但有时需要）

```csharp
public partial class WorkflowEditorWindow : Window
{
    private readonly ICanvasManager _canvasManager;

    public WorkflowEditorWindow(ICanvasManager canvasManager)
    {
        InitializeComponent();
        _canvasManager = canvasManager;
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "工作流文件 (*.json)|*.json" };
        if (dialog.ShowDialog() == true)
        {
            // ✅ CanvasManager只负责加载数据
            var workflow = await _canvasManager.LoadWorkflowFromFileAsync(dialog.FileName);

            // ✅ Code-Behind负责将数据绑定到UI
            WorkflowCanvas.DataContext = workflow;
        }
    }
}
```

## 数据流向

### 加载流程

```
用户操作
  ↓
点击"打开"按钮
  ↓
LoadCommand.Execute()
  ↓
ViewModel调用CanvasManager.LoadWorkflowFromFileAsync()
  ↓
CanvasManager读取JSON文件
  ↓
CanvasManager调用WorkflowSerializer反序列化
  ↓
返回StepListNodes对象
  ↓
ViewModel设置Workflow属性
  ↓
PropertyChanged事件触发
  ↓
WPF DataBinding自动更新UI
  ↓
WorkflowStepListNodes.xaml渲染工作流
```

### 保存流程

```
用户操作
  ↓
点击"保存"按钮
  ↓
SaveCommand.Execute()
  ↓
ViewModel调用CanvasManager.SetWorkflowRoot(Workflow)
  ↓
ViewModel调用CanvasManager.SaveWorkflowToFileAsync()
  ↓
CanvasManager调用WorkflowSerializer序列化
  ↓
生成JSON字符串
  ↓
CanvasManager写入文件
  ↓
完成
```

## Canvas的真正作用

Canvas实际上是**XAML中的布局容器**，用于**绝对定位**子元素。在本项目中：

### ✅ Canvas应该在哪里？

在**View层（XAML）**中：

```xml
<!-- WorkflowStepListNodes.xaml -->
<UserControl prism:ViewModelLocator.AutoWireViewModel="True">
    <Grid Width="{Binding Width}" Height="{Binding Height}">
        <!-- ✅ Canvas在这里，用于绝对定位 -->
        <ItemsControl ItemsSource="{Binding Connections}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <Canvas />  <!-- ✅ Canvas作为布局容器 -->
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
        </ItemsControl>

        <ItemsControl ItemsSource="{Binding Nodes}">
            <ItemsControl.ItemContainerStyle>
                <Style TargetType="ContentPresenter">
                    <!-- ✅ 通过DataBinding设置位置 -->
                    <Setter Property="Canvas.Left" Value="{Binding Position.X}" />
                    <Setter Property="Canvas.Top" Value="{Binding Position.Y}" />
                </Style>
            </ItemsControl.ItemContainerStyle>
        </ItemsControl>
    </Grid>
</UserControl>
```

### ❌ Canvas不应该在哪里？

在**Service层（CanvasManager）**中：

```csharp
// ❌ 错误示例
public class CanvasManager
{
    private Canvas _canvas;  // ❌ 业务层不应持有UI控件

    public void RenderWorkflow(StepListNodes workflow)
    {
        _canvas.Children.Clear();  // ❌ 业务层不应操作UI
        foreach (var node in workflow.Nodes)
        {
            _canvas.Children.Add(CreateNodeControl(node));  // ❌ 错误
        }
    }
}
```

## 为什么原来的设计有Canvas？

可能的原因：

1. **误解MVVM模式**：以为需要在代码中手动创建UI元素
2. **来自WinForms思维**：WinForms需要手动Add控件到容器
3. **未充分利用DataBinding**：不知道WPF可以通过绑定自动渲染

实际上，**WPF的DataBinding已经自动处理了所有渲染工作**：

```
数据变化（StepListNodes.Nodes.Add()）
  ↓
INotifyPropertyChanged / ObservableCollection
  ↓
WPF绑定引擎监听到变化
  ↓
自动触发ItemsControl重新渲染
  ↓
根据DataTemplate自动创建对应的UI控件
  ↓
根据Canvas.Left/Top自动定位
  ↓
UI更新完成
```

**无需手动操作Canvas！**

## 最佳实践

### ✅ DO

1. **CanvasManager只管理数据**
   ```csharp
   var workflow = _canvasManager.LoadWorkflowFromJson(json);
   ```

2. **ViewModel负责UI绑定**
   ```csharp
   Workflow = workflow;  // 属性变更触发UI更新
   ```

3. **XAML通过DataBinding渲染**
   ```xml
   <ItemsControl ItemsSource="{Binding Workflow.Nodes}" />
   ```

4. **依赖注入服务**
   ```csharp
   containerRegistry.RegisterSingleton<ICanvasManager, CanvasManager>();
   ```

### ❌ DON'T

1. **不要在Service层持有UI引用**
   ```csharp
   private Canvas _canvas;  // ❌
   ```

2. **不要在Service层操作UI**
   ```csharp
   _canvas.DataContext = workflow;  // ❌
   ```

3. **不要在Service层创建UI控件**
   ```csharp
   var button = new Button();  // ❌
   _canvas.Children.Add(button);  // ❌
   ```

4. **不要混合业务逻辑和UI逻辑**
   ```csharp
   public void SaveWorkflow()
   {
       var json = Serialize();
       File.WriteAllText(path, json);
       MessageBox.Show("保存成功");  // ❌ Service层不应弹窗
   }
   ```

## 单元测试示例

因为CanvasManager不依赖UI，所以可以轻松测试：

```csharp
[Fact]
public async Task LoadWorkflowFromFile_ShouldDeserializeCorrectly()
{
    // Arrange
    var serializer = new WorkflowSerializer();
    var eventAggregator = new EventAggregator();
    var canvasManager = new CanvasManager(serializer, eventAggregator);

    var originalWorkflow = CreateTestWorkflow();
    var json = serializer.SerializeToJson(originalWorkflow);
    await File.WriteAllTextAsync("test.json", json);

    // Act
    var loadedWorkflow = await canvasManager.LoadWorkflowFromFileAsync("test.json");

    // Assert
    Assert.NotNull(loadedWorkflow);
    Assert.Equal(originalWorkflow.Nodes.Count, loadedWorkflow.Nodes.Count);
    Assert.Equal(originalWorkflow.Width, loadedWorkflow.Width);
}
```

## 总结

### 为什么不需要Canvas？

1. **架构层次**：CanvasManager是Service层，Canvas是View层，两者不应混合
2. **职责分离**：CanvasManager负责数据，View负责渲染
3. **自动绑定**：WPF的DataBinding自动处理UI更新，无需手动操作
4. **可测试性**：不依赖UI控件，可以在单元测试中使用

### 正确的分工

- **CanvasManager**：加载数据 → 返回StepListNodes
- **ViewModel**：接收StepListNodes → 设置属性 → 触发PropertyChanged
- **View (XAML)**：监听PropertyChanged → DataBinding自动更新UI
- **Canvas**：作为XAML中的布局容器，通过Binding自动定位子元素

### 核心理念

**数据驱动UI，而非代码操作UI**

这就是WPF MVVM的精髓！
