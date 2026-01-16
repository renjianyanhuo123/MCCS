using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Threading;

using MCCS.Infrastructure.Communication;
using MCCS.Infrastructure.Services;
using MCCS.Interface.Components.Attributes;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Models.ParamterModels;
using MCCS.Interface.Components.ViewModels.Parameters;

namespace MCCS.Interface.Components.ViewModels
{
    /// <summary>
    /// 数据监控组件 ViewModel
    /// 高性能实现：100ms批量UI更新，支持高频数据采集
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
        private const int RefreshIntervalMs = 100;
        #endregion

        #region Fields
        /// <summary>
        /// 用于存储最新数据的高性能并发字典
        /// Key: ChannelId, Value: 最新值
        /// </summary>
        private readonly ConcurrentDictionary<long, double> _latestValues = new();

        /// <summary>
        /// 通道ID到模型的快速查找字典
        /// </summary>
        private readonly Dictionary<long, ProjectDataMonitorComponentItemModel> _channelToModel = [];

        /// <summary>
        /// 订阅清理容器
        /// </summary>
        private readonly CompositeDisposable _subscriptions = new();

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
        /// 构造函数 - 支持 DI 注入和业务参数
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
            _latestValues.Clear();

            if (parameters == null || parameters.Count == 0)
                return;

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
                _latestValues[model.Id] = 0.0;
            }
        }
        #endregion

        #region Data Update Methods
        /// <summary>
        /// 启动数据更新
        /// </summary>
        public void StartDataUpdates()
        {
            if (_isRunning || _isDisposed)
                return;

            _isRunning = true;

            // 订阅数据服务
            SubscribeToDataService();

            // 启动UI刷新定时器
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

            // 停止定时器
            StopRefreshTimer();

            // 清理订阅
            _subscriptions.Clear();
        }

        /// <summary>
        /// 订阅数据服务
        /// 使用高性能的批量数据接收策略
        /// </summary>
        private void SubscribeToDataService()
        {
            if (_channelToModel.Count == 0)
                return;

            var channelIds = _channelToModel.Keys.ToList();
            var dataService = ChannelDataServiceProvider.Instance;

            // 订阅指定通道的数据流
            // 数据直接写入ConcurrentDictionary，无需锁
            var subscription = dataService
                .GetChannelDataStream(channelIds)
                .Subscribe(OnDataReceived);

            _subscriptions.Add(subscription);
        }

        /// <summary>
        /// 数据接收处理（在后台线程执行）
        /// 仅更新缓存，不触发UI更新
        /// </summary>
        /// <param name="data">通道数据</param>
        private void OnDataReceived(ChannelDataItem data)
        {
            // 直接更新并发字典，无锁操作
            _latestValues[data.ChannelId] = data.Value;
        }

        /// <summary>
        /// 启动UI刷新定时器
        /// </summary>
        private void StartRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(RefreshIntervalMs)
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

            // 批量更新所有模型的显示值
            foreach (var kvp in _channelToModel)
            {
                if (_latestValues.TryGetValue(kvp.Key, out var value))
                {
                    var model = kvp.Value;
                    // 直接更新内部值（不触发通知）
                    model.UpdateValueDirect(value);
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
            _subscriptions.Dispose();
            _channelToModel.Clear();
            _latestValues.Clear();
        }
        #endregion
    }
}
