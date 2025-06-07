using MCCS.Services.Model3DService;
using MCCS.ViewModels.Others;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using MCCS.Services.Model3DService.EventParameters;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using HelixToolkit.SharpDX.Core;

namespace MCCS.ViewModels.Pages
{
    public class TestStartingPageViewModel : BaseViewModel
    {
        public const string Tag = "TestStartPage";
        
        #region private field 
        private bool _isLoading = true;
        private string _loadingMessage;
        private ObservableCollection<Model3DViewModel> _models;
        private Model3DViewModel _hoveredModel;

        private Camera _camera;
        private IEffectsManager _effectsManager;
        private readonly IModel3DLoaderService _model3DLoaderService;
        private CancellationTokenSource _loadingCancellation;
        #endregion

        #region Command

        public AsyncDelegateCommand LoadModelsCommand => new(LoadModelsAsync);
        public DelegateCommand<object> Model3DMouseDownCommand => new(OnModel3DMouseDown);
        public DelegateCommand<object> Model3DMouseMoveCommand => new(OnModel3DMouseMove);
        public DelegateCommand<object> Model3DMouseLeaveCommand => new(OnModel3DMouseLeave);
        public DelegateCommand ClearSelectionCommand => new(ClearSelection);

        #endregion

        public TestStartingPageViewModel(
            IEffectsManager effectsManager,
            IEventAggregator eventAggregator,
            IModel3DLoaderService model3DLoaderService,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        { 
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
            _effectsManager = effectsManager;
        }

        #region Property
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

        public SceneNodeGroupModel3D GroupModel { get; } = new();

        private ImportProgressEventArgs _loadingProgress;
        public ImportProgressEventArgs LoadingProgress
        {
            get => _loadingProgress;
            set => SetProperty(ref _loadingProgress, value);
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

                // UI线程更新
                foreach (var wrapper in wrappers)
                {
                    Models.Add(wrapper);
                    GroupModel.AddNode(wrapper.SceneNode);
                }
            }
            catch (OperationCanceledException)
            {
                // 加载被取消
            }
            catch (Exception ex)
            {
                // _eventAggregator.GetEvent<ErrorEvent>().Publish($"加载模型失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnModel3DMouseDown(object parameter)
        {
            if (parameter is Model3DViewModel model && model.IsSelectable)
            {
                model.IsSelected = !model.IsSelected;
            }
        }

        private void OnModel3DMouseMove(object parameter)
        {
            // 清除之前的悬停状态
            if (_hoveredModel != null && _hoveredModel != parameter)
            {
                _hoveredModel.IsHovered = false;
            }

            if (parameter is Model3DViewModel model && model.IsSelectable)
            {
                model.IsHovered = true;
                _hoveredModel = model;
            }
        }

        private void OnModel3DMouseLeave(object parameter)
        {
            if (parameter is Model3DViewModel model)
            {
                model.IsHovered = false;
                if (_hoveredModel == model)
                {
                    _hoveredModel = null;
                }
            }
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
        #endregion

    }
}
