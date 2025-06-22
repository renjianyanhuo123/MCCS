using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using MCCS.Core.Devices;
using MCCS.Core.Devices.Details;
using MCCS.Services.Model3DService;
using MCCS.Services.Model3DService.EventParameters;
using MCCS.ViewModels.Others;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using MCCS.Core.Devices.Manager;
using MCCS.Events;
using MCCS.ViewModels.Others.Controllers;
using MCCS.ViewModels.Pages.Controllers;
using MCCS.Events.Controllers;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MCCS.Models;
using LiveChartsCore.Kernel;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using System.Diagnostics;

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

        private Camera _camera;
        private IEffectsManager _effectsManager;
        private readonly IModel3DLoaderService _model3DLoaderService;
        private readonly IRegionManager _regionManager;
        private CancellationTokenSource? _loadingCancellation;
        private ImportProgressEventArgs _loadingProgress; 

        private DateTime _lastMouseMoveTime = DateTime.MinValue;
        private const int MouseMoveThrottleMs = 50; // 50ms 节流 
        private const int MaxPoints = 20;

        private readonly IDeviceManager _deviceManager;
        private readonly Random _random = new();
        private DateTime _currentTime = DateTime.Now;
        private float _sumPostion = 0;
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
        //public DelegateCommand OpenRightDrawerCommand => new(ExecuteOpenRightDrawerCommand);
        #endregion

        public TestStartingPageViewModel(
            IRegionManager regionManager,
            IDeviceManager deviceManager,
            IEffectsManager effectsManager,
            IEventAggregator eventAggregator,
            IModel3DLoaderService model3DLoaderService,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _deviceManager = deviceManager;
            _model3DLoaderService = model3DLoaderService;
            Models = []; 
            // Initialize camera
            _camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera()
            {
                LookDirection = new Vector3D(-100, -100, -100),
                Position = new Point3D(100, 100, 100),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 10000,
                NearPlaneDistance = 0.1f
            };
            _eventAggregator.GetEvent<InverseControlEvent>().Subscribe(RevicedInverseControlEvent);
            _eventAggregator.GetEvent<ReceivedCommandDataEvent>().Subscribe(OnReceivedMovePositionCommand);
            _effectsManager = effectsManager; 
            _regionManager = regionManager;

            CurveSeries2 = [
                new LineSeries<TimeAndMeasureValueModel>()
                {
                    Values = ObservableValues2,
                    Mapping = (model, index) =>
                    {
                        return new Coordinate(model.Time, model.Value);
                    },
                    Fill = null
                }
            ];
            CurveSeries1 = [
                new LineSeries<TimeAndMeasureValueModel>()
                {
                    Values = ObservableValues1,
                    Mapping = (model, index) =>
                    {
                        return new Coordinate(model.Time, model.Value);
                    },
                    Fill = null
                }
            ];
            XAxes =
            [
                new() {
                    Name = "Time(s)"
                }
            ];
            YAxes =
            [
                new() {
                    Name = "kN",
                    MinLimit = 9,
                    MaxLimit = 11
                }
            ]; 
        }

        #region Property
        /// <summary>
        /// 曲线一数据集合
        /// </summary>
        private ObservableCollection<TimeAndMeasureValueModel> ObservableValues1 { get; set; } = [];
        /// <summary>
        /// 曲线二数据集合
        /// </summary>
        private ObservableCollection<TimeAndMeasureValueModel> ObservableValues2 { get; set; } = [];
        public LiveChartsCore.SkiaSharpView.Axis[] XAxes { get; set; }
        public LiveChartsCore.SkiaSharpView.Axis[] YAxes { get; set; }
        /// <summary>
        /// 曲线一数据集合
        /// </summary>
        public ObservableCollection<ISeries> CurveSeries1 { get; set; }
        /// <summary>
        /// 曲线二数据集合
        /// </summary>
        public ObservableCollection<ISeries> CurveSeries2 { get; set; }
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
            var testModel = Models.FirstOrDefault(c => c.Model3DData.Key == "12853688641a40b58af7faf9ebe464f8");
            if (testModel != null) 
            {
                testModel.PositionChange(new SharpDX.Vector3(0, -1, 0), 0.1f);
                testModel.SceneNode.UpdateAllTransformMatrix();
            }
            _sumPostion += 0.1f;
            Debug.WriteLine($"模型位置: {_sumPostion}");
        }

        private void ExecuteLoadCommand() 
        {
            // _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(ControllerMainPageViewModel.Tag, UriKind.Relative));
        }

        private void ExecuteStartTestCommand()
        {
            IsStartedTest = !IsStartedTest;
        }

        private void RevicedInverseControlEvent(InverseControlEventParam param)
        {
            if (param == null) return;
            // 处理控制事件
            var targetModel = Models.FirstOrDefault(m => m.Model3DData.DeviceId == param.DeviceId);
            if (targetModel != null)
            {
                targetModel.IsSelected = false; 
            }
        }

        public void InitializeCurveDataSubscriptions()
        {
            _currentTime = DateTime.Now;
            var actor1 = _deviceManager.GetDevice("a1af5b38688247a58d3a9011bab98f81")?.DataStream
                ?? throw new ArgumentNullException($"Device id {"a1af5b38688247a58d3a9011bab98f81"} is not exist！");
            var actor2 = _deviceManager.GetDevice("b32bfa58d691427f86d339a2e3c9a596")?.DataStream
                ?? throw new ArgumentNullException($"Device id {"b32bfa58d691427f86d339a2e3c9a596"} is not exist！");
            actor1.Sample(TimeSpan.FromSeconds(1))
                .Subscribe(x => {
                    ObservableValues1.Add(new TimeAndMeasureValueModel
                    {
                        Time = (DateTime.Now - _currentTime).TotalSeconds,
                        Value = x.Value is MockActuatorCollection v ? v.Force : 0
                    });
                    if (ObservableValues1.Count > MaxPoints) ObservableValues1.RemoveAt(0);
                });
            actor2.Sample(TimeSpan.FromSeconds(1))
                .Subscribe(x => {
                    ObservableValues2.Add(new TimeAndMeasureValueModel
                    {
                        Time = (DateTime.Now - _currentTime).TotalSeconds,
                        Value = x.Value is MockActuatorCollection v ? v.Force : 0
                    });
                    if (ObservableValues2.Count > MaxPoints) ObservableValues2.RemoveAt(0);
                });
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
        private void OnReceivedMovePositionCommand(ReceivedCommandDataEventParam param) 
        {
            Task.Run(async () =>
            {
                const string targetModelKey = "12853688641a40b58af7faf9ebe464f8";
                var testModel = Models.FirstOrDefault(c => c.Model3DData.Key == targetModelKey);
                if (testModel == null) return;

                var lastGap = double.MaxValue;
                var stepSize = (float)param.Speed / 600.0f; // 60.0f * 0.1f 合并

                while (true)
                {
                    var currentGap = Math.Abs(_sumPostion * 10.0 - param.Target);
                    if (currentGap >= lastGap) break;

                    lastGap = currentGap;
                    var direction = Math.Sign(param.Target - _sumPostion * 10);
                    var movement = stepSize * direction;

                    testModel.PositionChange(new SharpDX.Vector3(0, -1, 0), movement);
                    testModel.SceneNode.UpdateAllTransformMatrix();
                    _sumPostion += movement;

                    await Task.Delay(1000);
                }

                // 达到最小差距后，直接设置为目标值
                var finalMovement = (float)(param.Target / 10.0 - _sumPostion);
                testModel.PositionChange(new SharpDX.Vector3(0, -1, 0), finalMovement);
                testModel.SceneNode.UpdateAllTransformMatrix();
                _sumPostion = (float)(param.Target / 10.0);
            });
        }

        /// <summary>
        /// 左键单击选择事件
        /// </summary>
        /// <param name="parameter"></param>
        private void OnModel3DMouseDown(object parameter)
        {
            if (parameter is not MouseDown3DEventArgs args) return;
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
            if (clickedModel is { IsSelectable: true })
            {
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
                //_eventAggregator.GetEvent<ControlEvent>().Publish(new ControlEventParam
                //{
                //    ChannelId = clickedModel.Model3DData.DeviceId ?? throw new ArgumentNullException("ControlEvent no ChannelId"),
                //    ChannelName = clickedModel.Model3DData.Name
                //});
                var channelsParam = new NavigationParameters
                {
                    { "ChannelId", clickedModel.Model3DData.DeviceId ?? throw new ArgumentNullException("ControlEvent no ChannelId")},
                    { "ChannelName", clickedModel.Model3DData.Name }
                };
                _regionManager.RequestNavigate(GlobalConstant.RightFlyoutRegionName, new Uri(ControllerMainPageViewModel.Tag, UriKind.Relative), channelsParam);
                // IsOpenFlyout = Models.Any(c => c.IsSelected);
            }
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
