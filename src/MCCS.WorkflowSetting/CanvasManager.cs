using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization;

namespace MCCS.WorkflowSetting
{
    /// <summary>
    /// 画布管理器实现
    /// 负责工作流的渲染、保存和加载
    /// </summary>
    public sealed class CanvasManager : ICanvasManager
    {
        private readonly IWorkflowSerializer _workflowSerializer;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService? _dialogService;

        private Canvas? _canvas;
        private StepListNodes? _currentWorkflowRoot;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="workflowSerializer">工作流序列化器</param>
        /// <param name="eventAggregator">事件聚合器</param>
        /// <param name="dialogService">对话框服务（可选）</param>
        public CanvasManager(
            IWorkflowSerializer workflowSerializer,
            IEventAggregator eventAggregator,
            IDialogService? dialogService = null)
        {
            _workflowSerializer = workflowSerializer ?? throw new ArgumentNullException(nameof(workflowSerializer));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dialogService = dialogService;
        }

        /// <summary>
        /// 初始化画布管理器
        /// </summary>
        public void Inititial(Canvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        /// <summary>
        /// 从JSON字符串渲染工作流
        /// </summary>
        public void RenderWorkflowByJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON字符串不能为空", nameof(json));

            try
            {
                // 使用序列化器反序列化工作流
                var workflowRoot = _workflowSerializer.DeserializeFromJson(json, _eventAggregator, _dialogService);

                // 保存当前工作流根节点
                _currentWorkflowRoot = workflowRoot;

                // 如果画布已初始化，可以在这里设置DataContext或进行其他UI绑定
                // 注：实际的UI渲染通常由XAML的数据绑定自动完成
                // 这里只是提供了一个扩展点
                if (_canvas != null)
                {
                    // 可选：触发画布重绘或其他UI更新操作
                    _canvas.DataContext = workflowRoot;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"渲染工作流失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保存当前工作流为JSON字符串
        /// </summary>
        public string SaveWorkflowToJson(string workflowName = "")
        {
            if (_currentWorkflowRoot == null)
                throw new InvalidOperationException("当前没有可保存的工作流");

            try
            {
                return _workflowSerializer.SerializeToJson(_currentWorkflowRoot, workflowName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"保存工作流失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保存当前工作流到文件
        /// </summary>
        public async Task SaveWorkflowToFileAsync(string filePath, string workflowName = "")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("文件路径不能为空", nameof(filePath));

            if (_currentWorkflowRoot == null)
                throw new InvalidOperationException("当前没有可保存的工作流");

            try
            {
                var json = SaveWorkflowToJson(workflowName);

                // 确保目录存在
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 异步写入文件
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"保存工作流到文件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从文件加载工作流
        /// </summary>
        public async Task LoadWorkflowFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("文件路径不能为空", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"找不到工作流文件: {filePath}");

            try
            {
                // 异步读取文件
                var json = await File.ReadAllTextAsync(filePath);

                // 渲染工作流
                RenderWorkflowByJson(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"从文件加载工作流失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 设置当前工作流根节点
        /// </summary>
        public void SetWorkflowRoot(StepListNodes rootNode)
        {
            _currentWorkflowRoot = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
        }

        /// <summary>
        /// 获取当前工作流根节点
        /// </summary>
        public StepListNodes? GetWorkflowRoot()
        {
            return _currentWorkflowRoot;
        }

        /// <summary>
        /// 添加节点（保留用于未来扩展）
        /// </summary>
        public void Add()
        {
            // 预留方法，用于未来可能的节点添加功能
            throw new NotImplementedException("此功能尚未实现");
        }
    }
}
