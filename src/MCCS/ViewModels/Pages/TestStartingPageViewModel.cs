using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using MaterialDesignThemes.Wpf;
using MCCS.Common;
using MCCS.Core.Devices;
using MCCS.Core.Devices.Commands;
using MCCS.Core.Devices.Details;
using MCCS.Core.Devices.Manager;
using MCCS.Events.ControlCommand;
using MCCS.Events.Controllers;
using MCCS.Models;
using MCCS.Models.ControlCommand;
using MCCS.Models.Model3D;
using MCCS.Services.Model3DService;
using MCCS.Services.Model3DService.EventParameters;
using MCCS.ViewModels.Others;
using MCCS.ViewModels.Others.Controllers;
using MCCS.ViewModels.Pages.Controllers;
using MCCS.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace MCCS.ViewModels.Pages
{
    public class TestStartingPageViewModel : BaseViewModel
    {
        public const string Tag = "TestStartPage";
        
        #region private field 
        private bool _isLoading = true;
        private bool _isStartedTest = false; // 是否开始试验
        private string _loadingMessage = "";

        private ObservableCollection<Model3DViewModel> _models;
        private Model3DViewModel? _lastHoveredModel = null;
        private ObservableCollection<ControlProcessExpander> _controlProcessExpanders = [];

        private Camera _camera;
        private IEffectsManager _effectsManager;
        private readonly IModel3DLoaderService _model3DLoaderService;
        private readonly IRegionManager _regionManager;
        private readonly IContainerProvider _containerProvider;
        private CancellationTokenSource? _loadingCancellation;
        private ImportProgressEventArgs _loadingProgress = new(); 

        private DateTime _lastMouseMoveTime = DateTime.MinValue;
        private const int MouseMoveThrottleMs = 50; // 50ms 节流 
        private const int MaxPoints = 20;

        private readonly IDeviceManager _deviceManager;
        private readonly Random _random = new();
        private DateTime _currentTime = DateTime.Now;

        private Model3DViewModel? _clearOperationModel = null;
        //private SceneNodeGroupModel3D _dataLabelsGroup;
        //private SceneNodeGroupModel3D _connectionLinesGroup;
        #endregion

        #region Command
        public AsyncDelegateCommand LoadModelsCommand => new(LoadModelsAsync);
        public DelegateCommand<object> Model3DMouseDownCommand => new(OnModel3DMouseDown);
        public DelegateCommand<object> Model3DMouseMoveCommand => new(OnModel3DMouseMove);
        public DelegateCommand ClearSelectionCommand => new(ClearSelection);
        public DelegateCommand StartTestCommand => new(ExecuteStartTestCommand);
        public DelegateCommand LoadCommand => new(ExecuteLoadCommand);
        public DelegateCommand TestModelMoveCommand => new(ExecuteTestModelMoveCommand);
        public DelegateCommand<MouseDown3DEventArgs> Model3DRightMouseDownCommand => new(ExecuteModel3DRightMouseDownCommand);
        public DelegateCommand DisplacementClearCommand => new(ExecuteDisplacementClearCommand);
        public DelegateCommand ForceClearCommand => new(ExecuteForceClearCommand);
        public AsyncDelegateCommand<string> SetCurveCommand => new(ExecuteSetCurveCommand);
        #endregion

        public TestStartingPageViewModel(
            IContainerProvider containerProvider,
            IRegionManager regionManager,
            IDeviceManager deviceManager,
            IEffectsManager effectsManager,
            IEventAggregator eventAggregator,
            IModel3DLoaderService model3DLoaderService,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            EnvironmentMap = TextureModel.Create(@"F:\models\test\Cubemap_Grandcanyon.dds");
            _containerProvider = containerProvider;
            _deviceManager = deviceManager;
            _model3DLoaderService = model3DLoaderService;
            _models = []; 
            // Initialize camera
            _camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera()
            {
                LookDirection = new Vector3D(-100, -100, -100),
                Position = new Point3D(100, 100, 100),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 10000,
                NearPlaneDistance = 0.1f
            };
            _eventAggregator.GetEvent<InverseControlEvent>().Subscribe(ReceivedInverseControlEvent);
            _eventAggregator.GetEvent<ReceivedCommandDataEvent>().Subscribe(OnReceivedCommand);
            _eventAggregator.GetEvent<NotificationCommandStopedEvent>().Subscribe(x => 
            {
                var model = Models.FirstOrDefault(s => s.Model3DData.DeviceId == x.CommandId);
                if (model == null) return;
                model.CancelModelMove.Cancel();
                model.CancelModelMove.Dispose();
                model.CancelModelMove = new CancellationTokenSource();
            });
            _effectsManager = effectsManager; 
            _regionManager = regionManager;
        }

        #region Property
        /// <summary>
        /// 背景环境配置
        /// </summary>
        public TextureModel EnvironmentMap { get; private set; }

        /// <summary>
        /// 是否显示右键菜单
        /// </summary>
        private bool _isShowContextMenu = false;
        public bool IsShowContextMenu 
        {
            get => _isShowContextMenu;
            set => SetProperty(ref _isShowContextMenu, value);
        }

        private bool _isOpenContextMenu = false;
        public bool IsOpenContextMenu 
        {
            get => _isOpenContextMenu;
            set => SetProperty(ref _isOpenContextMenu, value);
        }

        /// <summary>
        /// 曲线数据集合
        /// </summary>
        public ObservableCollection<CurveShowModel> CurveModels { get; set; } = [];
        public bool IsStartedTest
        {
            get => _isStartedTest;
            set => SetProperty(ref _isStartedTest, value);
        }

        public Camera Camera
        {
            get => _camera;
            set => SetProperty(ref _camera, value);
        }

        public IEffectsManager EffectsManager
        {
            get => _effectsManager;
            protected set => SetProperty(ref _effectsManager, value);
        }

        /// <summary>
        /// 采集数据显示的标签
        /// </summary>
        public BillboardText3D CollectionDataLabels { get; } = new() 
        { 
            IsDynamic = true
        };

        public LineGeometry3D ConnectionLineGeometry { get; } = new LineGeometry3D() 
        {
            IsDynamic = true
        };

        public SceneNodeGroupModel3D GroupModel { get; } = new();
        
        public ImportProgressEventArgs LoadingProgress
        {
            get => _loadingProgress;
            set
            {
                if (SetProperty(ref _loadingProgress, value))
                {
                    LoadingMessage = $"模型加载中...{Math.Round(_loadingProgress.ProgressPercentage, 2)}%";
                }
            }
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        } 

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public ObservableCollection<Model3DViewModel> Models
        {
            get => _models;
            set => SetProperty(ref _models, value);
        }
        public ObservableCollection<ControlProcessExpander> ControlProcessExpanders 
        {
            get => _controlProcessExpanders;
            set => SetProperty(ref _controlProcessExpanders, value);
        }
        #endregion

        #region private method
        private async Task LoadModelsAsync()
        {
            _loadingCancellation = new CancellationTokenSource();
            IsLoading = true;
            try
            {
                // 清理旧模型
                ClearModels();

                var progress = new Progress<ImportProgressEventArgs>(p => LoadingProgress = p);
                var wrappers = await _model3DLoaderService.ImportModelsAsync(progress, _loadingCancellation.Token);

                var positions = new Vector3Collection();
                var connectionIndexs = new IntCollection();

                // UI线程更新
                foreach (var wrapper in wrappers)
                {
                    Models.Add(wrapper);
                    GroupModel.AddNode(wrapper.SceneNode);
                    if(wrapper.DataLabels.Count > 0) 
                        CollectionDataLabels.TextInfo.AddRange(wrapper.DataLabels);
                    // 重新整合线段; 默认没有一个点连接两条线的情况.
                    if (wrapper.ConnectPoints.Count > 0 
                        && wrapper.ConnectCollection.Count > 0 
                        && wrapper.ConnectPoints.Count == wrapper.ConnectCollection.Count) 
                    {
                        foreach (var index in wrapper.ConnectCollection)
                        {
                            positions.Add(wrapper.ConnectPoints[index]);
                            connectionIndexs.Add(connectionIndexs.Count);
                        }
                    }
                }
                // 渲染时机很重要,这里相当于首次设置
                ConnectionLineGeometry.Positions = positions;
                ConnectionLineGeometry.Indices = connectionIndexs;
            }
            catch (OperationCanceledException)
            {
                // 加载被取消
            }
            catch (Exception)
            {
                // _eventAggregator.GetEvent<ErrorEvent>().Publish($"加载模型失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// 测试使用
        /// </summary>
        private void ExecuteTestModelMoveCommand() 
        {
            var testModel = Models.FirstOrDefault(c => c.Model3DData.Key == "ad129785d8494c759cbe6ede6e28f3cd");
            if (testModel == null) return;
            testModel.PositionChange(new SharpDX.Vector3(0, -1, 0), 0.1f);
            testModel.SceneNode.UpdateAllTransformMatrix();
            testModel.DisplacementOffsetValue += 0.1f;
            Debug.WriteLine($"模型位置: {testModel.DisplacementOffsetValue}");
        }

        private void ExecuteLoadCommand() 
        {
            // _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(ControllerMainPageViewModel.Tag, UriKind.Relative));
        }

        private void ExecuteStartTestCommand()
        {
            IsStartedTest = !IsStartedTest;
        }

        private void ReceivedInverseControlEvent(InverseControlEventParam param)
        {
            if (param == null) return;
            // 处理控制事件
            var targetModel = Models.FirstOrDefault(m => m.Model3DData.DeviceId == param.DeviceId);
            if (targetModel != null)
            {
                targetModel.IsSelected = false; 
            }
        }

        /// <summary>
        /// 初始化曲线
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void InitialCurves() 
        {
            var devices= Models.Where(s => s.Model3DData.DeviceId != null).Select(s => s.Model3DData).ToList();
            _currentTime = DateTime.Now;
            foreach (var device in devices) 
            {
                var displacementModel = new CurveShowModel("Time(s)", "kN")
                {
                    DeviceId = device.DeviceId ?? string.Empty,
                    ExpanderHeaderName = $"{device.Name}_时间-位移曲线",
                };
                var forceModel = new CurveShowModel("Time(s)", "mm")
                {
                    DeviceId = device.DeviceId ?? string.Empty,
                    ExpanderHeaderName = $"{device.Name}_时间-力曲线",
                };
                CurveModels.Add(displacementModel);
                CurveModels.Add(forceModel);
                var actor = _deviceManager.GetDevice(device.DeviceId ?? string.Empty)?.DataStream
                ?? throw new ArgumentNullException($"Device id {device.DeviceId} is not exist！"); 
                actor.Sample(TimeSpan.FromSeconds(1))
                .Subscribe(x => {
                    displacementModel.ObservableValues.Add(new CurveMeasureValueModel
                    {
                        XValue = (DateTime.Now - _currentTime).TotalSeconds,
                        YValue = x.Value is MockActuatorCollection v ? v.Displacement : 0
                    });
                    if (displacementModel.ObservableValues.Count > MaxPoints) displacementModel.ObservableValues.RemoveAt(0);
                });
                actor.Sample(TimeSpan.FromSeconds(1))
                .Subscribe(x => {
                    forceModel.ObservableValues.Add(new CurveMeasureValueModel
                    {
                        XValue = (DateTime.Now - _currentTime).TotalSeconds,
                        YValue = x.Value is MockActuatorCollection v ? v.Force : 0
                    });
                    if (forceModel.ObservableValues.Count > MaxPoints) forceModel.ObservableValues.RemoveAt(0);
                });
            }
        }
        /// <summary>
        /// 初始化数据订阅
        /// </summary>
        public void InitializeDataSubscriptions()
        {
            var actor1 = _deviceManager.GetDevice("a1af5b38688247a58d3a9011bab98f81")?.DataStream 
                ?? throw new ArgumentNullException($"Device id {"a1af5b38688247a58d3a9011bab98f81"} is not exist！");
            var actor2 = _deviceManager.GetDevice("b32bfa58d691427f86d339a2e3c9a596")?.DataStream 
                ?? throw new ArgumentNullException($"Device id {"b32bfa58d691427f86d339a2e3c9a596"} is not exist！");
            actor1
                .Sample(TimeSpan.FromSeconds(0.8)) 
                .Subscribe(OnDeviceDataReceived);

            actor2
                .Sample(TimeSpan.FromSeconds(0.8))
                .Subscribe(OnDeviceDataReceived);
        } 

        /// <summary>
        /// 处理设备数据更新
        /// </summary>
        /// <param name="deviceData">设备数据</param>
        private void OnDeviceDataReceived(DeviceData deviceData)
        {
            // 在UI线程更新模型
            var targetModel = Models.FirstOrDefault(m => m.Model3DData.DeviceId == deviceData.DeviceId);
            if (targetModel != null)
            {
                UpdateModelWithDeviceData(targetModel, deviceData);
            }
        }

        /// <summary>
        /// 使用设备数据更新模型
        /// </summary>
        /// <param name="model">要更新的模型</param>
        /// <param name="deviceData">设备数据</param>
        private void UpdateModelWithDeviceData(Model3DViewModel model, DeviceData deviceData)
        {
            if (deviceData.Value is MockActuatorCollection v) 
            {
                // Debug.WriteLine($"更新模型: {model.Model3DData.DeviceId}, 力: {v.Force}, 位移: {v.Displacement}");
                model.ForceNum = v.Force;
                model.DisplacementNum = v.Displacement;
                CollectionDataLabels.Invalidate();
            }
        }

        /// <summary>
        /// 接受到信息后移动
        /// </summary>
        /// <param name="param"></param>
        private async void OnReceivedCommand(ReceivedCommandDataEventParam param)
        {
            if (param.Param == null) throw new ArgumentNullException(nameof(param.Param));
           var expander = new ControlProcessExpander(param.Param)
            {
                CommandId = param.ChannelIds.Count == 1 ? param.ChannelIds.First() : Guid.NewGuid().ToString("N"),
                CommandName = param.ChannelIds.Count == 1 ? Models.First(s => s.Model3DData.DeviceId == param.ChannelIds.First()).Model3DData.Name : "组合控制",
                ProgressRate = 0.0,
                ControlType = param.ChannelIds.Count == 1 ? ControlTypeEnum.Single : ControlTypeEnum.Combine,
                ControlMode = param.ControlMode
            };
            ControlProcessExpanders.Add(expander);
            var targetModel = Models.FirstOrDefault(s => s.Model3DData.DeviceId == expander.CommandId)
                ?? throw new ArgumentNullException();
            switch (param.ControlMode)
            {
                case ControlMode.Manual:
                    break;
                case ControlMode.Static:
                    if (param.Param is StaticControlModel model
                        && model.UnitType == ControlUnitTypeEnum.Displacement)
                    {
                        StaticMode_ModelMovePosition(model, targetModel);
                    }
                    break;
                case ControlMode.Programmable:
                    break;
                case ControlMode.Fatigue:
                    break;
                default:
                    break;
            }
            var device = _deviceManager.GetDevice(expander.CommandId) ?? throw new ArgumentNullException(nameof(expander.CommandId));
            await device.SendCommandAsync(new DeviceCommand
            {
                DeviceId = expander.CommandId,
                Type = CommandTypeEnum.SetMove,
                Parameters = expander.GetParamDic()
            }, targetModel.CancelModelMove.Token);

            // 事件完成后订阅
            targetModel.DeviceSubscription = device.CommandStatusStream
                .Sample(TimeSpan.FromSeconds(1.5))
                .Subscribe(OnChangedCommandStatus);
        }

        private void OnChangedCommandStatus(CommandResponse response) 
        {
            var expander = ControlProcessExpanders.FirstOrDefault(c => c.CommandId == response.CommandId);
            var targetModel = Models.FirstOrDefault(s => s.Model3DData.DeviceId == response.CommandId);
            if (expander == null || targetModel == null) return;
            expander.ProgressRate = Math.Round(response.Progress * 100.0, 2);
            // Debug.WriteLine($"更新进度: {Math.Round(response.Progress * 100.0, 2)}");
            if (response.CommandExecuteStatus == CommandExecuteStatusEnum.ExecuttionCompleted || response.CommandExecuteStatus == CommandExecuteStatusEnum.Stoping) 
            {
                // 解绑状态订阅链接
                targetModel.DeviceSubscription?.Dispose();
                // 通知控制界面这条指令已经完成
                _eventAggregator.GetEvent<NotificationCommandFinishedEvent>().Publish(new NotificationCommandStatusEventParam 
                { 
                    CommandId = response.CommandId,
                    CommandExecuteStatus = response.CommandExecuteStatus
                }); 
                PropertyChangeAsync(() => 
                {
                    ControlProcessExpanders.Remove(expander);
                });
            }
        }

        /// <summary>
        /// 静态控制模式下 模型移动
        /// </summary>
        /// <param name="param"></param>
        private void StaticMode_ModelMovePosition(StaticControlModel param, Model3DViewModel testModel)
        {
            var cancelToken = testModel.CancelModelMove.Token;
            Task.Run(async () =>
            {
                if (testModel == null) return; 
                var lastGap = double.MaxValue;
                var stepSize = (float)param.Speed / 600.0f; // 60.0f * 0.1f 合并
                var moveDirection = testModel.Model3DData.Orientation?.ToVector<SharpDX.Vector3>() ?? new SharpDX.Vector3(0, -1, 0);
                while (true)
                {
                    // 检查取消
                    testModel.CancelModelMove.Token.ThrowIfCancellationRequested();
                    var currentGap = Math.Abs(testModel.DisplacementOffsetValue * 10.0 - param.TargetValue);
                    if (currentGap >= lastGap) break;

                    lastGap = currentGap;
                    var direction = Math.Sign(param.TargetValue - testModel.DisplacementOffsetValue * 10);
                    var movement = stepSize * direction;

                    testModel.PositionChange(moveDirection, movement);
                    testModel.SceneNode.UpdateAllTransformMatrix();
                    testModel.DisplacementOffsetValue += movement;

                    await Task.Delay(1000, testModel.CancelModelMove.Token);
                }

                // 达到最小差距后，直接设置为目标值
                var finalMovement = (float)(param.TargetValue / 10.0 - testModel.DisplacementOffsetValue);
                testModel.PositionChange(moveDirection, finalMovement);
                testModel.SceneNode.UpdateAllTransformMatrix();
                testModel.DisplacementOffsetValue = (float)(param.TargetValue / 10.0);
            }, cancelToken);
        }

        /// <summary>
        /// 左键单击选择事件
        /// </summary>
        /// <param name="parameter"></param>
        private void OnModel3DMouseDown(object parameter)
        {
            if (parameter is not MouseDown3DEventArgs args) return;
            IsShowContextMenu = false;
            // 检查是否按住了修饰键（如果按住修饰键，则不处理选择）
            if (Keyboard.IsKeyDown(Key.LeftShift) ||
                Keyboard.IsKeyDown(Key.RightShift) ||
                Keyboard.IsKeyDown(Key.LeftAlt) ||
                Keyboard.IsKeyDown(Key.RightAlt) ||
                Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl)) return;
            if (args.OriginalInputEventArgs is MouseButtonEventArgs mouseArgs &&
                mouseArgs.LeftButton != MouseButtonState.Pressed) return;
            if (!IsStartedTest) return;
            // 获取点击的模型
            var clickedModel = args.HitTestResult != null ? FindViewModel(args.HitTestResult.ModelHit) : null; 
            // 如果点击到了模型
            if (clickedModel is not { IsSelectable: true }) return;
            // 切换当前模型的选中状态
            clickedModel.IsSelected = true;
            // IsRightDrawerOpen = true;
            var channels = Models
                .Where(s => s.IsSelected)
                .Select(c => new ControllerItemModel
                {
                    ChannelId = c.Model3DData.DeviceId ?? string.Empty,
                    ChannelName = c.Model3DData.Name ?? string.Empty,
                }).ToList();
            var channelsParam = new NavigationParameters
            {
                { "ChannelId", clickedModel.Model3DData.DeviceId ?? throw new ArgumentNullException("ControlEvent no ChannelId")},
                { "ChannelName", clickedModel.Model3DData.Name }
            };
            _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(ControllerMainPageViewModel.Tag, UriKind.Relative), channelsParam);
        }
        /// <summary>
        /// 单独处理鼠标右键弹窗
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteModel3DRightMouseDownCommand(MouseDown3DEventArgs e)
        {
            if (e?.OriginalInputEventArgs is not MouseButtonEventArgs { ChangedButton: MouseButton.Right } mouseArgs) return;
            if (e.HitTestResult?.ModelHit == null) return;
            var model = e.HitTestResult != null ? FindViewModel(e.HitTestResult.ModelHit) : null;
            if (model == null) return;
            _clearOperationModel = model;
            IsShowContextMenu = true;
            mouseArgs.Handled = true;
        }
        
        /// <summary>
        /// 设置曲线
        /// </summary>
        private async Task ExecuteSetCurveCommand(string id)
        {
            var curveModel = CurveModels.FirstOrDefault(c => c.DeviceId == id);
            if(curveModel == null) return;
            var setCurveView = _containerProvider.Resolve<SetCurveDialog>();
            var result = await DialogHost.Show(setCurveView,"RootDialog");
            Debug.WriteLine("Test!!!");
        }

        /// <summary>
        /// 位移清零
        /// </summary>
        private void ExecuteDisplacementClearCommand() 
        {
            IsShowContextMenu = false;
        }
        /// <summary>
        /// 力清零
        /// </summary>
        private void ExecuteForceClearCommand() 
        {
            if (_clearOperationModel != null) 
            {
                //_clearOperationModel.
            }
            IsShowContextMenu = false;
        }

        /// <summary>
        /// 鼠标移动事件处理
        /// </summary>
        /// <param name="parameter"></param>
        private void OnModel3DMouseMove(object? parameter)
        {
            if (parameter is not HitTestResult res) return;

            // 节流处理，避免过于频繁的更新
            var now = DateTime.Now;
            if ((now - _lastMouseMoveTime).TotalMilliseconds < MouseMoveThrottleMs) return;
            _lastMouseMoveTime = now;
            var hoveredModel = FindViewModel(res.ModelHit);
            // 清除之前的悬停状态
            if (hoveredModel != null)
            {
                hoveredModel.IsHovered = true;
            }
            if (_lastHoveredModel != hoveredModel && _lastHoveredModel != null)
            {
                _lastHoveredModel.IsHovered = false;
            }
            _lastHoveredModel = hoveredModel;
        }

        private void ClearSelection()
        {
            foreach (var model in Models.Where(m => m.IsSelected))
            {
                model.IsSelected = false;
            }
        }

        private void ClearModels()
        {
            foreach (var model in Models)
            {
                model.SceneNode?.Dispose();
            }
            Models.Clear();
            GroupModel.Clear();
            ClearSelection();
        }

        /// <summary>
        /// 寻找Tag中的Model3D对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Model3DViewModel? FindViewModel(object obj)
        {
            var hitNode = obj as SceneNode;
            if (obj is not SceneNode node) return null;
            // 通过Tag找到对应的ViewModel
            while (hitNode != null)
            {
                if (hitNode.Tag is Model3DViewModel vm)
                {
                    return vm; 
                }
                hitNode = hitNode.Parent;
            }
            return null;
        } 
        #endregion

    }
}
