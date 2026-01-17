using System.Collections.ObjectModel;
using System.Windows.Threading;

using MCCS.Infrastructure.Services;
using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Models.ParamterModels;
using MCCS.Interface.Components.ViewModels.Parameters;

namespace MCCS.Interface.Components.ViewModels
{
    /// <summary>
    /// 数据监控组件 ViewModel
    /// 高性能实现：100ms批量UI更新，利用ChannelDataService的缓存机制
    /// </summary>
    [InterfaceComponent(
        "data-monitor-component",
        "数据监控组件",
        InterfaceComponentCategory.Display,
        Description = "用于实时监控和显示测试数据",
        Icon = "MonitorDashboard",
        IsCanSetParam = true,
        SetParamViewName = nameof(DataMonitorSetParamPageViewModel),
        Order = 2)]
    public sealed class ProjectDataMonitorComponentPageViewModel : BaseComponentViewModel, IDisposable
    {
        #region Constants
        /// <summary>
        /// UI刷新间隔（毫秒）
        /// </summary>
        private const int _refreshIntervalMs = 100;
        #endregion

        #region Fields
        /// <summary>
        /// 通道ID到模型的快速查找字典
        /// </summary>
        private readonly Dictionary<long, ProjectDataMonitorComponentItemModel> _channelToModel = [];

        /// <summary>
        /// 所有需要监控的通道ID列表（缓存，避免每次刷新时重新创建）
        /// </summary>
        private long[] _channelIds = [];

        /// <summary>
        /// UI刷新定时器
        /// </summary>
        private DispatcherTimer? _refreshTimer;

        /// <summary>
        /// 是否已释放
        /// </summary>
        private volatile bool _isDisposed;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        private volatile bool _isRunning;
        #endregion

        #region Properties
        /// <summary>
        /// 数据监控项集合
        /// </summary>
        public ObservableCollection<ProjectDataMonitorComponentItemModel> Chilldren { get; } = [];

        /// <summary>
        /// 是否正在更新数据
        /// </summary>
        public bool IsRunning => _isRunning;
        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="parameters">业务参数（从外部传入）</param>
        public ProjectDataMonitorComponentPageViewModel(
            List<DataMonitorSettingItemParamModel> parameters)
        {
            InitializeItems(parameters);
            StartDataUpdates();
        }

        /// <summary>
        /// 初始化监控项
        /// </summary>
        private void InitializeItems(List<DataMonitorSettingItemParamModel>? parameters)
        {
            Chilldren.Clear();
            _channelToModel.Clear();

            if (parameters == null || parameters.Count == 0)
            {
                _channelIds = [];
                return;
            }

            foreach (var param in parameters)
            {
                var model = new ProjectDataMonitorComponentItemModel
                {
                    Id = param.PseudoChannel.Id,
                    DisplayName = param.PseudoChannel.DisplayName,
                    Unit = param.PseudoChannel.Unit,
                    RetainBit = param.RetainBit,
                    Value = 0.0
                };

                Chilldren.Add(model);
                _channelToModel[model.Id] = model;
            }

            // 缓存通道ID数组，避免每次刷新时重新分配
            _channelIds = [.. _channelToModel.Keys];
        }
        #endregion

        #region Data Update Methods
        /// <summary>
        /// 启动数据更新
        /// </summary>
        public void StartDataUpdates()
        {
            if (_isRunning || _isDisposed || _channelIds.Length == 0)
                return;

            _isRunning = true;
            StartRefreshTimer();
        }

        /// <summary>
        /// 停止数据更新
        /// </summary>
        public void StopDataUpdates()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            StopRefreshTimer();
        }

        /// <summary>
        /// 启动UI刷新定时器
        /// </summary>
        private void StartRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(_refreshIntervalMs)
            };
            _refreshTimer.Tick += OnRefreshTimerTick;
            _refreshTimer.Start();
        }

        /// <summary>
        /// 停止UI刷新定时器
        /// </summary>
        private void StopRefreshTimer()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Tick -= OnRefreshTimerTick;
                _refreshTimer = null;
            }
        }

        /// <summary>
        /// 定时器触发 - 批量刷新UI
        /// 在UI线程执行，每100ms触发一次
        /// </summary>
        private void OnRefreshTimerTick(object? sender, EventArgs e)
        {
            if (_isDisposed || !_isRunning)
                return;

            // 从ChannelDataService获取所有通道的最新值
            var dataService = ChannelDataServiceProvider.Instance;
            var currentValues = dataService.GetCurrentValues(_channelIds);

            // 批量更新所有模型的显示值
            foreach (var kvp in currentValues)
            {
                if (_channelToModel.TryGetValue(kvp.Key, out var model))
                {
                    // 直接更新内部值（不触发通知）
                    model.UpdateValueDirect(kvp.Value);
                    // 刷新显示（仅在值变化时触发通知）
                    model.RefreshDisplay();
                }
            }
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            StopDataUpdates();
            _channelToModel.Clear();
        }
        #endregion
    }
}
