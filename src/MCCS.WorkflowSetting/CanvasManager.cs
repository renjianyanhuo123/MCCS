using System;
using System.IO;
using System.Threading.Tasks;
using MCCS.WorkflowSetting.Models.Nodes;
using MCCS.WorkflowSetting.Serialization;

namespace MCCS.WorkflowSetting
{
    /// <summary>
    /// 工作流管理器实现
    /// 负责工作流的序列化、反序列化和状态管理
    /// 注意：此类只负责数据层面的操作，不涉及UI层面的Canvas操作
    /// UI绑定应该由ViewModel或View层处理
    /// </summary>
    public sealed class CanvasManager : ICanvasManager
    {
        private readonly IWorkflowSerializer _workflowSerializer;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService? _dialogService;

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
        /// 从JSON字符串加载并还原工作流
        /// </summary>
        /// <param name="json">工作流JSON字符串</param>
        /// <returns>还原的工作流根节点</returns>
        public StepListNodes LoadWorkflowFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON字符串不能为空", nameof(json));

            try
            {
                // 使用序列化器反序列化工作流
                var workflowRoot = _workflowSerializer.DeserializeFromJson(json, _eventAggregator, _dialogService);

                // 保存当前工作流根节点（用于后续保存操作）
                _currentWorkflowRoot = workflowRoot;

                // 返回工作流根节点，由调用者（ViewModel/View）负责UI绑定
                return workflowRoot;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载工作流失败: {ex.Message}", ex);
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
        /// <param name="filePath">文件路径</param>
        /// <returns>加载的工作流根节点</returns>
        public async Task<StepListNodes> LoadWorkflowFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("文件路径不能为空", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"找不到工作流文件: {filePath}");

            try
            {
                // 异步读取文件
                var json = await File.ReadAllTextAsync(filePath);

                // 加载并返回工作流
                return LoadWorkflowFromJson(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"从文件加载工作流失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 设置当前工作流根节点（用于保存操作）
        /// </summary>
        /// <param name="rootNode">根节点</param>
        public void SetWorkflowRoot(StepListNodes rootNode)
        {
            _currentWorkflowRoot = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
        }

        /// <summary>
        /// 获取当前工作流根节点
        /// </summary>
        /// <returns>当前工作流根节点，如果未设置则返回null</returns>
        public StepListNodes? GetWorkflowRoot()
        {
            return _currentWorkflowRoot;
        }
    }
}
