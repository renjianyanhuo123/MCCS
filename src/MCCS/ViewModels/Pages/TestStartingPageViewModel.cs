using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using MCCS.Collecter.ControlChannelManagers;
using MCCS.Collecter.ControllerManagers;
using MCCS.Collecter.PseudoChannelManagers;
using MCCS.Collecter.SignalManagers;
using MCCS.Common;
using MCCS.Common.DataManagers;
using MCCS.Common.DataManagers.CurrentTest;
using MCCS.Components.GlobalNotification.Models;
using MCCS.Events.Controllers;
using MCCS.Events.Tests;
using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Helper;
using MCCS.Infrastructure.Models.Model3D;
using MCCS.Infrastructure.Models.ProjectManager;
using MCCS.Infrastructure.Repositories;
using MCCS.Infrastructure.Repositories.Project;
using MCCS.Infrastructure.TestModels;
using MCCS.Models.ControlCommand;
using MCCS.Models.CurveModels;
using MCCS.Models.Model3D;
using MCCS.Services.Model3DService;
using MCCS.Services.Model3DService.EventParameters;
using MCCS.Services.NotificationService;
using MCCS.ViewModels.Dialogs;
using MCCS.ViewModels.Others;
using MCCS.ViewModels.Pages.Controllers;
using Microsoft.Extensions.Configuration;
using Serilog;
using SharpDX;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Color = System.Windows.Media.Color;
using HitTestResult = HelixToolkit.SharpDX.Core.HitTestResult;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace MCCS.ViewModels.Pages
{
    public class TestStartingPageViewModel : BaseViewModel
    {
        public const string Tag = "TestStartPage";
        
        #region private field  
        private string _loadingMessage = ""; 

        private Model3DViewModel? _lastHoveredModel = null;
        private ObservableCollection<ControlProcessExpander> _controlProcessExpanders = [];

        private Camera _camera = new OrthographicCamera();
        private IEffectsManager _effectsManager;
        private readonly IModel3DLoaderService _model3DLoaderService; 
        private readonly IModel3DDataRepository _model3DDataRepository; 
        private readonly IControllerManager _controllerManager;
        private readonly IControlChannelManager _controlChannelManager;
        private readonly IPseudoChannelManager _pseudoChannelManager;
        private readonly INotificationService _notificationService;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectDbContext _projectDbContext; 
        private readonly ISignalManager _signalManager;
        private readonly IProjectDataRecordRepository _projectDataRecordRepository;
        private readonly string _defaultSavePath;

        private IDisposable? _combinedSubscription; 
        private IDisposable? _dataRecordSaveSubscription;
        private readonly Subject<Unit> _stopSignal = new();

        // 存储所有的广告牌引用,方便后期快速更新
        private readonly Dictionary<long, TextInfoExt> _textInfoDic = [];
        private readonly Dictionary<string, TextInfoExt> _modelTextInfoDic = [];

        private CancellationTokenSource? _loadingCancellation;
        private ImportProgressEventArgs _loadingProgress = new(); 

        private DateTime _lastMouseMoveTime = DateTime.MinValue;
        private const int _mouseMoveThrottleMs = 50; // 50ms 节流 
        private const int _maxPoints = 20; 

        private Model3DViewModel? _clearOperationModel = null; 
        private MultipleControllerMainPageViewModel? _multipleControllerMainPageViewModel = null;
        private bool _isCtrlPressed = false;

        private long _testStartFlag = 1;
        #endregion

        #region Command
        public AsyncDelegateCommand LoadModelsCommand { get; }
        public DelegateCommand<object> Model3DMouseDownCommand { get; }
        public DelegateCommand<object> Model3DMouseMoveCommand { get; }
        public DelegateCommand ClearSelectionCommand { get; }
        public AsyncDelegateCommand StartTestCommand { get; }
        public DelegateCommand PauseTestCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand TestModelMoveCommand { get; }
        public DelegateCommand<MouseDown3DEventArgs> Model3DRightMouseDownCommand { get; }
        public DelegateCommand DisplacementClearCommand { get; }
        public DelegateCommand ForceClearCommand { get; }
        public AsyncDelegateCommand<string> SetCurveCommand { get; }
        public DelegateCommand<KeyEventArgs> CtrlKeyDownCommand { get; }
        public DelegateCommand<KeyEventArgs> CtrlKeyUpCommand { get; }
        public DelegateCommand ShowCurveCommand { get; }
        public DelegateCommand TestMoveModel3D { get; }

        #endregion

        public TestStartingPageViewModel( 
            IEffectsManager effectsManager,
            IEventAggregator eventAggregator,
            IModel3DLoaderService model3DLoaderService,
            IControllerManager controllerManager,
            IControlChannelManager controlChannelManager,
            IModel3DDataRepository model3DDataRepository,
            INotificationService notificationService,
            IPseudoChannelManager pseudoChannelManager,
            IProjectRepository projectRepository,
            IProjectDbContext projectDbContext, 
            IConfiguration configuration,
            ISignalManager signalManager,
            IProjectDataRecordRepository projectDataRecordRepository,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            // EnvironmentMap = TextureModel.Create(@"F:\models\test\Cubemap_Grandcanyon.dds"); 
            _model3DLoaderService = model3DLoaderService;
            _controllerManager = controllerManager;
            _model3DDataRepository = model3DDataRepository; 
            _effectsManager = effectsManager;
            _notificationService = notificationService;
            _controlChannelManager = controlChannelManager;
            _pseudoChannelManager = pseudoChannelManager;
            _projectRepository = projectRepository;
            _projectDbContext = projectDbContext;
            _signalManager = signalManager;
            _projectDataRecordRepository = projectDataRecordRepository;
            _defaultSavePath = configuration["DefaultProjectSavePath"] ?? "";
            LoadModelsCommand = new AsyncDelegateCommand(LoadModelsAsync);
            Model3DMouseDownCommand = new DelegateCommand<object>(OnModel3DMouseDown);
            Model3DMouseMoveCommand = new DelegateCommand<object>(OnModel3DMouseMove);
            ClearSelectionCommand = new DelegateCommand(ClearSelection);
            StartTestCommand = new AsyncDelegateCommand(ExecuteStartTestCommand);
            PauseTestCommand = new DelegateCommand(ExecutePauseTestCommand);
            LoadCommand = new DelegateCommand(ExecuteLoadCommand);
            TestModelMoveCommand = new DelegateCommand(ExecuteTestModelMoveCommand);
            Model3DRightMouseDownCommand = new DelegateCommand<MouseDown3DEventArgs>(ExecuteModel3DRightMouseDownCommand);
            DisplacementClearCommand = new DelegateCommand(ExecuteDisplacementClearCommand);
            ForceClearCommand = new DelegateCommand(ExecuteForceClearCommand);
            SetCurveCommand = new AsyncDelegateCommand<string>(ExecuteSetCurveCommand);
            CtrlKeyDownCommand = new DelegateCommand<KeyEventArgs>(OnCtrlKeyDownCommand);
            CtrlKeyUpCommand = new DelegateCommand<KeyEventArgs>(OnCtrlKeyUpCommand);
            ShowCurveCommand = new DelegateCommand(ExecuteShowCurveCommand);
            TestMoveModel3D = new DelegateCommand(StaticMode_ModelMovePosition);
            _eventAggregator.GetEvent<InverseControlEvent>().Subscribe(ReceivedInverseControlEvent);
            _eventAggregator.GetEvent<ReceivedCommandDataEvent>().Subscribe(OnReceivedCommand);
            _eventAggregator.GetEvent<UnLockCommandEvent>().Subscribe(ReceivedUnLockEvent);
            _eventAggregator.GetEvent<NotificationCommandStopedEvent>().Subscribe(x => 
            {
                //var model = Models.FirstOrDefault(s => s.Model3DData.DeviceId == x.CommandId);
                //if (model == null) return;
                //model.CancelModelMove.Cancel();
                //model.CancelModelMove.Dispose();
                //model.CancelModelMove = new CancellationTokenSource();
            });
            _eventAggregator.GetEvent<OperationValveEvent>().Subscribe(OnOperationValveEvent);
            _eventAggregator.GetEvent<OperationTareEvent>().Subscribe(param =>
            {
                if (param.ControlType == 0) _notificationService.Show("成功", "位移清零成功", NotificationType.Success);
                else _notificationService.Show("失败", "力清零成功", NotificationType.Success);
            });
        }

        #region Property
        /// <summary>
        /// 背景环境配置
        /// </summary>
        public TextureModel? EnvironmentMap { get; private set; }

        /// <summary>
        /// 是否显示右键菜单
        /// </summary>
        private bool _isShowContextMenu = false;
        public bool IsShowContextMenu 
        {
            get => _isShowContextMenu;
            set => SetProperty(ref _isShowContextMenu, value);
        }  
        /// <summary>
        /// 曲线数据集合
        /// </summary>
        public ObservableCollection<CurveShowModel> CurveModels { get; set; } = [];

        /// <summary>
        /// 是否开始试验
        /// </summary>
        private bool _isStartedTeste = false; 
        public bool IsStartedTest
        {
            get => _isStartedTeste;
            set => SetProperty(ref _isStartedTeste, value);
        }
        /// <summary>
        /// 是否暂停
        /// 这是整个停止的按钮
        /// </summary>
        private bool _isPaused = false; 
        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public Camera Camera
        {
            get => _camera;
            set => SetProperty(ref _camera, value);
        }

        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
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
        /// <summary>
        /// 线集合
        /// </summary>
        public LineGeometry3D ConnectionLineGeometry { get; } = new() 
        {
            IsDynamic = true
        };

        #region 状态显示模型 
        // TODO: 暂时先保留后面如果用到
        // public BillboardSingleImage3D BillboardModel { private set; get; }
        public SharpDX.Matrix[] BillboardInstances { private set; get; } = [];
        public BillboardInstanceParameter[] BillboardInstanceParams { private set; get; } = [];
        #endregion

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

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public List<Model3DViewModel> Models { get; private set; } = [];

        public ObservableCollection<ControlProcessExpander> ControlProcessExpanders 
        {
            get => _controlProcessExpanders;
            set => SetProperty(ref _controlProcessExpanders, value);
        }

        public ObservableCollection<object> Controllers { get; } = [];
        private bool _isShowController;

        public bool IsShowController
        {
            get => _isShowController;
            set => SetProperty(ref _isShowController, value);
        }
        #endregion

        #region private method
        private void ExecuteShowCurveCommand()
        {
            var parameters = new DialogParameters() { { "title", "曲线图像" } };
            _dialogService.Show(SetCurveDialogViewModel.Tag, parameters, res => { });
        }

        private async Task LoadModelsAsync()
        {
            _loadingCancellation = new CancellationTokenSource();
            IsLoading = true;
            if (GlobalDataManager.Instance.StationSiteInfo == null) throw new ArgumentNullException("StationSiteInfo is NULL");
            var modelAggregate = await _model3DDataRepository.GetModelAggregateByStationIdAsync(GlobalDataManager.Instance.StationSiteInfo.Id);
            if (modelAggregate == null) throw new ArgumentNullException("ModelAggregate is NULL");
            if (modelAggregate.BaseInfo == null) throw new ArgumentNullException("ModelAggregate.BaseInfo is NULL");
            Camera = new OrthographicCamera()
            {
                LookDirection = modelAggregate.BaseInfo.CameraLookDirection.ToVector<Vector3D>(),
                Position = modelAggregate.BaseInfo.CameraPosition.ToVector<Point3D>(),
                UpDirection = modelAggregate.BaseInfo.CameraUpDirection.ToVector<Vector3D>(),
                FarPlaneDistance = modelAggregate.BaseInfo.FarPlaneDistance,
                NearPlaneDistance = modelAggregate.BaseInfo.NearPlaneDistance,
                Width = modelAggregate.BaseInfo.FieldViewWidth
            };
            BackgroundColor = (Color)ColorConverter.ConvertFromString(modelAggregate.BaseInfo.CameraBackgroundColor);
            try
            {
                // 清理旧模型
                ClearModels();
                var progress = new Progress<ImportProgressEventArgs>(p => LoadingProgress = p);
                var wrappers = await _model3DLoaderService.ImportModelsAsync(
                    modelAggregate.Model3DDataList, 
                    progress, 
                    (Color)ColorConverter.ConvertFromString(modelAggregate.BaseInfo.MaterialColor), 
                    _loadingCancellation.Token);
                var positions = new Vector3Collection();
                var connectionIndexs = new IntCollection(); 
                // UI线程更新
                foreach (var wrapper in wrappers)
                { 
                    Models.Add(wrapper);
                    GroupModel.AddNode(wrapper.SceneNode); 
                } 
                var index = 0; 
                var streamList = new List<IObservable<Model3DRenderModel>>();
                foreach (var billboardInfo in modelAggregate.BillboardInfos)
                {
                    var temp1 = (Color)ColorConverter.ConvertFromString(billboardInfo.BackgroundColor);
                    var temp2 = (Color)ColorConverter.ConvertFromString(billboardInfo.FontColor);
                    var backgroundColor = new SharpDX.Color(temp1.R, temp1.G, temp1.B, temp1.A);
                    var fontColor = new SharpDX.Color(temp2.R, temp2.G, temp2.B, temp2.A);
                    var bindModelViewModel = Models.FirstOrDefault(c => c.ModelId == billboardInfo.ModelFileId);
                    if (bindModelViewModel != null)
                    {
                        // 模型起点
                        positions.Add(bindModelViewModel.GetModelCenterFromGeometry());
                        // 广告牌中心
                        positions.Add(billboardInfo.PositionStr.ToVector<Vector3>());
                        connectionIndexs.AddRange([index, index + 1]); 
                        index += 2;
                    } 
                    var textInfo = new TextInfoExt
                    {
                        Text = billboardInfo.BillboardName,
                        Origin = billboardInfo.PositionStr.ToVector<Vector3>(),
                        Padding = new Vector4(5),
                        Foreground = fontColor,
                        Background = backgroundColor,
                        Scale = billboardInfo.Scale,
                        Size = billboardInfo.FontSize
                    };
                    if (billboardInfo.BillboardType == BillboardTypeEnum.DataShow)
                    {
                        var pseudoChannel = _pseudoChannelManager.GetPseudoChannelById(billboardInfo.PseudoChannelId); 
                        // 将流和对应的 textInfo 关联起来，添加到列表
                        var stream = pseudoChannel.GetPseudoChannelStream() 
                            .Select(data => new Model3DRenderModel
                            {
                                Model3DId = billboardInfo.ModelFileId,
                                Data = data,
                                BillboardId = billboardInfo.Id,
                                PseudoChannelId = billboardInfo.PseudoChannelId, 
                            }); 
                        _textInfoDic.Add(billboardInfo.Id, textInfo);
                        streamList.Add(stream);
                    }
                    else
                    {
                        var channel = GlobalDataManager.Instance.Model3Ds?.FirstOrDefault(c => c.Id == bindModelViewModel?.Model3DData.Id)?.ControlChannelInfos.FirstOrDefault();
                        if (channel != null)
                        {
                            var channelOperation = _controlChannelManager.GetControlChannel(channel.Id); 
                            if (channelOperation.ValveStatus == ValveStatusEnum.Closed)
                            {
                                textInfo.Text = "OFF";
                                _modelTextInfoDic.Add(billboardInfo.ModelFileId, textInfo);
                            }
                        } 
                    }
                    CollectionDataLabels.TextInfo.Add(textInfo); 
                }

                // 合并所有流，统一订阅
                if (streamList.Any())
                {
                    _combinedSubscription = streamList.CombineLatest()
                        .Sample(TimeSpan.FromMilliseconds(100))  
                        .ObserveOn(Scheduler.Default)
                        .Subscribe(ExecuteUpdateUI); 
                }
                // 渲染时机很重要,这里相当于首次设置
                ConnectionLineGeometry.Positions = positions;
                ConnectionLineGeometry.Indices = connectionIndexs; 
                // 初始化订阅链接
                // InitializeDataSubscriptions();  
            }
            catch (OperationCanceledException)
            {
                // 加载被取消
            }
            catch (Exception ex)
            { 
                Log.Error($"加载模型失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteUpdateUI(IList<Model3DRenderModel> allUpdates)
        {
#if DEBUG
            // Debug.WriteLine($"CombineLatest 批量更新 {allUpdates.Count} 个广告牌");
#endif
            // 一次性更新所有 TextInfo
            // TODO：这部分还需要大改，因为虚拟通道的类型没加上；以及对应的最大，最小值也没有加上
            foreach (var data in allUpdates)
            {
                _textInfoDic[data.BillboardId].Text = $"{ChangeToStr(data.Data.Unit)}: {data.Data.Value:F3}{data.Data.Unit}";
                if (data.Data.Unit != "mm") continue;
                // 更新模型位置
                var modelInfo = Models.First(c => c.ModelId == data.Model3DId);
                var moveDirection = modelInfo.Model3DData.Orientation?.ToVector<SharpDX.Vector3>() ?? new SharpDX.Vector3(0, -1, 0);
                var proportion = (data.Data.Value + 100) / 200.0f;
#if DEBUG
                Debug.WriteLine($"CombineLatest 批量更新 {allUpdates.Count} 个广告牌");
#endif
                modelInfo.SetPosition(moveDirection, proportion);
                modelInfo.SceneNode.UpdateAllTransformMatrix();
            }
            // 只调用一次 Invalidate
            CollectionDataLabels.Invalidate();
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
             
        }

        private async Task ExecuteStartTestCommand()
        {
            // TODO：后期如果更加复杂后，需要加入事务处理
            if (IsStartedTest)
            { 
                if ((DateTimeOffset.Now - GlobalDataManager.Instance.CurrentTestInfo.StartTime).Seconds < 3)
                {
                    _notificationService.Show("提示", "实验时间小于3S,请稍后......");
                    return;
                } 
                // 终止当前实验时,将会重置整个实验
                if (_controllerManager.OperationTest(false) && GlobalDataManager.Instance.CurrentTestInfo.Stop())
                {
                    _testStartFlag = 1;// 重置数据标志
                    _notificationService.Show("成功", "成功停止实验!", NotificationType.Success);
                    GlobalDataManager.Instance.SetValue(new CurrentTestInfo());
                    IsStartedTest = false;
                    // 停止采集信号
                    _stopSignal.OnNext(Unit.Default);
                    _stopSignal.OnCompleted();
                    _dataRecordSaveSubscription?.Dispose();
                    // 终止试验后取消所有的选中
                    Models.ForEach(c => c.IsSelected = false);
                    Controllers.Clear();
                    IsShowController = false;
                }
                else
                {
                    _notificationService.Show("失败", "未能正确停止实验,请检查!", NotificationType.Error);
                }
            }
            else
            {
                if (_controllerManager.OperationTest(true) && GlobalDataManager.Instance.CurrentTestInfo.Start())
                {
                    _testStartFlag = 1;
                    var name = $"{DateTime.Now:yyyy_MM_dd_hhmmsss}_Proj/";
                    var savePath = Path.Combine(_defaultSavePath, name);
                    // 开始记录数据
                    var projectSuccess = await _projectRepository.AddProjectAsync(new ProjectModel
                    {
                        Name = name,
                        Code = "111",
                        FilePath = Path.Combine(_defaultSavePath, name),
                        Standard = "",
                        MethodName = "",
                        StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    });
                    FileHelper.EnsureDirectoryExists(savePath);
                    _projectDbContext.Initial(Path.Combine(savePath, "project_data_record.dat"));
                    if (projectSuccess > 0)
                    {
                        _dataRecordSaveSubscription = _signalManager.GetProjectDataRecords()
                            .Buffer(TimeSpan.FromSeconds(5), 1000)
                            .TakeUntil(_stopSignal)
                            .Subscribe(async void (datas) =>
                            {
                                try
                                {
#if DEBUG
                                    Debug.WriteLine($"当前试验保存数据{datas.Count}个");
#endif
                                    var saveList = datas.SelectMany(x => x).ToList();
                                    foreach (var item in saveList) item.Timestamp = _testStartFlag++;
                                    await _projectDataRecordRepository.BatchAddRecordAsync(saveList);
                                }
                                catch (Exception e)
                                {
                                    Log.Error($"试验数据保存数据错误{e.Message}");
                                }
                            });
                        _notificationService.Show("成功", "成功开启实验!", NotificationType.Success);
                        IsStartedTest = true;
                    }
                }
                else
                {
                    _notificationService.Show("失败", "未能正确开启实验,请检查!", NotificationType.Error);
                }
            } 
        }

        /// <summary>
        /// 暂停/继续试验
        /// </summary>
        private void ExecutePauseTestCommand()
        {
            if (IsPaused)
            {
                GlobalDataManager.Instance.CurrentTestInfo?.Continue();
            }
            else
            {
                GlobalDataManager.Instance.CurrentTestInfo?.Pause();
            }
            IsPaused = !IsPaused;
        }

        /// <summary>
        /// 取消控制界面
        /// </summary>
        /// <param name="param"></param>
        private void ReceivedInverseControlEvent(InverseControlEventParam param)
        {
            if (param == null) return;
            // 处理控制事件
            var targetModel = Models.FirstOrDefault(m => m.Model3DData.Id == param.ModelId);
            if (targetModel != null)
            {
                targetModel.IsSelected = false; 
            }
            var removeObj = Controllers.FirstOrDefault(c => c is ControllerMainPageViewModel vm && vm.CurrentModelId == param.ModelId);
            if (removeObj != null)
            {
                Controllers.Remove(removeObj);
                IsShowController = Controllers.Count > 0;
            }
        }
        /// <summary>
        /// 接收到解锁事件
        /// </summary>
        /// <param name="param"></param>
        private void ReceivedUnLockEvent(UnLockCommandEventParam param)
        {
            // (1) 添加控制器
            var multipleControllerMainPageViewModel = Controllers.OfType<MultipleControllerMainPageViewModel>()
                .FirstOrDefault(s => s.CombineId == param.CombineId);
            if (multipleControllerMainPageViewModel == null) return;
            foreach (var item in multipleControllerMainPageViewModel.Children)
            {
                Controllers.Add(new ControllerMainPageViewModel(item.CurrentModelId, _controlChannelManager, _notificationService, _eventAggregator));
            }
            // (2) 移除掉组合控制器部分
            Controllers.Remove(multipleControllerMainPageViewModel);
        } 

        private void OnOperationValveEvent(OperationValveEventParam param)
        {
            if (param == null) return;
            if (_modelTextInfoDic.TryGetValue(param.ModelId, out var textInfo) && textInfo != null)
            {
                textInfo.Text = param.IsOpen ? "ON" : "OFF";
                IsShowContextMenu = false;
            }
        } 
        /// <summary>
        /// 接收到信息后移动
        /// </summary>
        /// <param name="param"></param>
        private async void OnReceivedCommand(ReceivedCommandDataEventParam param)
        {
            if (param.Param == null) throw new ArgumentNullException(nameof(param.Param)); 
        }

        private float _sumDistance = 0.0f;
        /// <summary>
        /// 静态控制模式下 模型移动
        /// </summary> 
        private void StaticMode_ModelMovePosition()
        { 
            var testModel = Models.First(c => c.Model3DData.Id == 439);
            var moveDirection = testModel.Model3DData.Orientation?.ToVector<SharpDX.Vector3>() ?? new SharpDX.Vector3(0, -1, 0);   
            testModel.PositionChange(moveDirection, -5f);
            _sumDistance -= 5f;
#if DEBUG
            Debug.WriteLine($"当前移动的距离: {_sumDistance:F2}");
#endif
            testModel.SceneNode.UpdateAllTransformMatrix();  
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
                Keyboard.IsKeyDown(Key.RightAlt)) return;
            if (args.OriginalInputEventArgs is MouseButtonEventArgs mouseArgs &&
                mouseArgs.LeftButton != MouseButtonState.Pressed) return; 
            // 获取点击的模型
            var modelId = args.HitTestResult != null ? FindViewModel(args.HitTestResult.ModelHit) : null; 
            // 如果点击到了模型
            var clickedModel = Models.FirstOrDefault(c => c.ModelId == modelId);
            if (clickedModel is not { IsSelectable: true }) return;
            // 如果已经参与控制则退出
            if (clickedModel.IsSelected) return;
            if (!IsStartedTest)
            {
                _notificationService.Show("提示", "试验未开始,请先开始试验!");
                return;
            }
            // 鼠标左键 + Ctrl 键单独处理
            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ExecuteLeftMouseCtrlDownCommand(clickedModel);
                return;
            }
            // 如果阀门关闭则退出
            var controlChannelInfo = GlobalDataManager.Instance.Model3Ds?.FirstOrDefault(s => s.Id == clickedModel.Model3DData.Id)?.ControlChannelInfos.FirstOrDefault();
            if (controlChannelInfo == null) throw new ArgumentNullException("模型没有绑定控制通道");
            var channel = _controlChannelManager.GetControlChannel(controlChannelInfo.Id);
            if (channel.ValveStatus != ValveStatusEnum.Opened)
            { 
                _notificationService.Show("提示", "请先右键单击打开该作动器的阀门");
                return;
            }
            // 切换当前模型的选中状态
            clickedModel.IsSelected = true;
            if (Controllers.OfType<ControllerMainPageViewModel>().All(c => c.CurrentModelId != clickedModel.Model3DData.Id)
                && !Controllers.OfType<MultipleControllerMainPageViewModel>().Any(c => c.Children.Any(s => s.CurrentModelId == clickedModel.Model3DData.Id)))
            {
                Controllers.Add(new ControllerMainPageViewModel(clickedModel.Model3DData.Id, _controlChannelManager, _notificationService, _eventAggregator));
                IsShowController = Controllers.Count > 0;
            }
        }
        /// <summary>
        /// 处理左键 + Ctrl 键按下事件
        /// </summary>
        /// <param name="model"></param>
        private void ExecuteLeftMouseCtrlDownCommand(Model3DViewModel model)
        {
            if (model.IsMultipleSelected) return;
            model.IsMultipleSelected = true;
            if (_multipleControllerMainPageViewModel != null)
            {
                _multipleControllerMainPageViewModel.Children.Add(new MultipleControllerChildPageViewModel(model.Model3DData.Id));
            }
        }
        /// <summary>
        /// 处理 Ctrl 键按下事件
        /// </summary>
        /// <param name="e"></param>
        private void OnCtrlKeyDownCommand(KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && !_isCtrlPressed)
            {
                _isCtrlPressed = true;
                _multipleControllerMainPageViewModel = new MultipleControllerMainPageViewModel(_eventAggregator);
            } 
            //TODO：这个地方一定要注意，会捕获全局的键盘事件
            // e.Handled = true;
        }
        /// <summary>
        /// 处理 Ctrl 键抬起事件
        /// </summary>
        /// <param name="e"></param>
        private void OnCtrlKeyUpCommand(KeyEventArgs e)
        {
            if ((e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl) || !_isCtrlPressed) return;
            _isCtrlPressed = false; // 重置状态
            if (_multipleControllerMainPageViewModel is { Children.Count: > 1 })
            {
                Controllers.Add(_multipleControllerMainPageViewModel);
                IsShowController = Controllers.Count > 0;
                foreach (var model in Models.Where(m => m.IsMultipleSelected))
                {
                    model.IsSelected = true;
                }
            }
            // 清除多选状态
            foreach (var model in Models.Where(m => m.IsMultipleSelected))
            {
                model.IsMultipleSelected = false;
            }
            _multipleControllerMainPageViewModel = null;
            e.Handled = true;
        }
        /// <summary>
        /// 单独处理鼠标右键弹窗
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteModel3DRightMouseDownCommand(MouseDown3DEventArgs e)
        {
            if (e?.OriginalInputEventArgs is not MouseButtonEventArgs { ChangedButton: MouseButton.Right } mouseArgs) return;
            if (e.HitTestResult?.ModelHit == null) return;
            var modelId = e.HitTestResult != null ? FindViewModel(e.HitTestResult.ModelHit) : null;
            if (modelId == null) return;
            _clearOperationModel = Models.FirstOrDefault(c => c.ModelId == modelId);
            var controlChannelInfo = GlobalDataManager.Instance.Model3Ds?.FirstOrDefault(s => s.ModelFileKey == modelId)?.ControlChannelInfos.FirstOrDefault();
            // 是否可控制
            if (controlChannelInfo != null && _clearOperationModel is { Model3DData.IsCanControl: true })
            {
                _eventAggregator.GetEvent<NotificationRightMenuValveStatusEvent>().Publish(new NotificationRightMenuValveStatusEventParam
                {
                    ControlChannelId = controlChannelInfo.Id,
                    ModelId = _clearOperationModel.ModelId
                });
                IsShowContextMenu = true;
            } 
            mouseArgs.Handled = true;
        }
        
        /// <summary>
        /// 设置曲线
        /// </summary>
        private async Task ExecuteSetCurveCommand(string id)
        {
            // var curveModel = CurveModels.FirstOrDefault(c => c.DeviceId == id);
            // if(curveModel == null) return;
            // var setCurveView = _containerProvider.Resolve<SetCurveDialog>();
            // var result = await DialogHost.Show(setCurveView,"RootDialog");
            // Debug.WriteLine("Test!!!");
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
            if ((now - _lastMouseMoveTime).TotalMilliseconds < _mouseMoveThrottleMs) return;
            _lastMouseMoveTime = now;
            var hoveredModelId = FindViewModel(res.ModelHit);
            var hoveredModel = Models.FirstOrDefault(c => c.ModelId == hoveredModelId);
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
            // 清理 GroupModel 中的节点
            // SceneNodeGroupModel3D 使用 Clear() 方法清空所有子节点
            GroupModel.Clear();

            // 如果 GroupModel 有 SceneNode 属性，遍历并清理 Tag
            if (GroupModel.SceneNode != null)
            {
                foreach (var child in GroupModel.SceneNode.Traverse())
                {
                    child.Tag = null;
                }
            }
            foreach (var model in Models)
            {
                model.Dispose();
            }
            Models.Clear();
            // 显式释放连接线资源
            ConnectionLineGeometry.Positions = null;
            ConnectionLineGeometry.Indices = null; 
            ClearSelection();
        } 
        /// <summary>
        /// 寻找Tag中的Model3D对象的ID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string? FindViewModel(object obj)
        {
            var hitNode = obj as SceneNode;
            if (obj is not SceneNode node) return null;
            // 通过Tag找到对应的ViewModel
            while (hitNode != null)
            {
                if (hitNode.Tag is string vm)
                {
                    return vm; 
                }
                hitNode = hitNode.Parent;
            }
            return null;
        }

        private static string ChangeToStr(string str)
        {
            return str switch
            {
                "mm" => "Position",
                "kN" => "Force",
                _ => string.Empty
            };
        }

        #endregion
         
    }
}
