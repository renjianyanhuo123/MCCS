
using HelixToolkit.Wpf.SharpDX;
using MCCS.Core.Models.Model3D;
using MCCS.Core.Repositories;
using MCCS.Services.Model3DService;
using MCCS.ViewModels.Others;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.SharpDX.Core.Assimp;
using SharpDX;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;

namespace MCCS.ViewModels.Pages
{
    public class TestStartingPageViewModel
        : BaseViewModel
    {
        public const string Tag = "TestStartPage";

        

        #region private field
        private ObservableCollection<Model3DViewModel> _model3DList;
        private bool _isLoading = false;
        private string _loadingMessage;
        private Model3DGroup _combinedModel;
        private ObservableCollection<Model3DViewModel> _models;
        private Model3DViewModel _hoveredModel;

        private readonly Camera _camera;
        private readonly IConfiguration _configuration;
        private readonly IModel3DDataRepository _model3DDataRepository;
        private readonly IModel3DLoaderService _model3DLoaderService;
        #endregion

        #region Command

        public AsyncDelegateCommand LoadModelsCommand { get; }
        public DelegateCommand Model3DMouseDownCommand { get; }
        public DelegateCommand Model3DMouseMoveCommand { get; }
        public DelegateCommand Model3DMouseLeaveCommand { get; }
        public DelegateCommand ClearSelectionCommand { get; }

        #endregion

        public TestStartingPageViewModel(
            IModel3DDataRepository model3DDataRepository,
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            IModel3DLoaderService model3DLoaderService,
            IDialogService dialogService) : base(eventAggregator, dialogService)
        {
            _configuration = configuration;
            _model3DDataRepository = model3DDataRepository;
            _model3DLoaderService = model3DLoaderService;
            Models = [];

            // Initialize camera
            _camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera()
            {
                Position = new Point3D(10, 10, 10),
                LookDirection = new Vector3D(-10, -10, -10),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 1000,
                NearPlaneDistance = 0.1,
                FieldOfView = 45
            };
        }

        #region Property

        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        public Model3DGroup CombinedModel
        {
            get => _combinedModel;
            set => SetProperty(ref _combinedModel, value);
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
            IsLoading = true;
            LoadingMessage = "正在加载模型...";
            try
            {
                var groupKey = _configuration["AppSettings:ModelKey"] 
                               ?? throw new ArgumentNullException("AppSettings:ModelKey");
                var model3DList = await _model3DDataRepository.GetModelAsync(groupKey);
                var loadTasks = model3DList.Select((modelData, index) =>
                    LoadSingleModelAsync(modelData, index, model3DList.Count));
            }
            catch (Exception ex)
            {
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = string.Empty;
            }
        }

        private async Task LoadSingleModelAsync(Model3DData modelData, int index, int total)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadingMessage = $"加载模型 {index + 1}/{total}: {Path.GetFileName(modelData.FilePath)}";
                });

                try
                {
                    // 使用Assimp加载模型
                    var importer = new Importer();
                    var scene = importer.Load(modelData.FilePath);

                    if (scene is { Root: not null })
                    {
                    }
                }
                catch (Exception ex)
                {
                    // 处理加载错误
                    System.Diagnostics.Debug.WriteLine($"Failed to load model {modelData.FilePath}: {ex.Message}");
                }
            });
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
        #endregion

    }
}
