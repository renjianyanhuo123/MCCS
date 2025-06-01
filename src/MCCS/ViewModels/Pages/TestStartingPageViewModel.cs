
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MCCS.Core.Repositories;
using MCCS.Services.Model3DService;
using MCCS.ViewModels.Others;
using Microsoft.Extensions.Configuration;

namespace MCCS.ViewModels.Pages
{
    public class TestStartingPageViewModel
        : BaseViewModel
    {
        public const string Tag = "TestStartPage";

        private bool _isLoaded = false;
        private Model3DGroup _combinedModel;
        private Model3DViewModel _hoveredModel;

        private readonly IConfiguration _configuration;
        private readonly IModel3DDataRepository _model3DDataRepository;
        private readonly IModel3DLoaderService _model3DLoaderService;

        public ObservableCollection<Model3DViewModel> Models { get; } = [];

        #region private field
        private ObservableCollection<Model3DViewModel> _model3DList;

        #endregion

        #region Command

        public AsyncDelegateCommand LoadModelsCommand => new(ExecuteLoadModelsCommand);
        public DelegateCommand<Model3DViewModel> ModelClickCommand { get; }
        public DelegateCommand<Model3DViewModel> ModelHoverCommand { get; }
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
        }

        #region Property
        public Model3DViewModel HoveredModel
        {
            get => _hoveredModel;
            set => SetProperty(ref _hoveredModel, value);
        }

        public Model3DGroup CombinedModel
        {
            get => _combinedModel;
            set => SetProperty(ref _combinedModel, value);
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            set => SetProperty(ref _isLoaded, value);
        }
        public ObservableCollection<Model3DViewModel> Model3DList
        {
            get => _model3DList;
            set => SetProperty(ref _model3DList, value);
        }
        #endregion

        #region private method
        private void CombineModels()
        {
            var modelGroup = new Model3DGroup();

            foreach (var modelVm in Models)
            {
                modelGroup.Children.Add(modelVm.Model);
            }

            // 添加光源
            modelGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));
            modelGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(1, 1, 1)));
            modelGroup.Children.Add(new AmbientLight(Color.FromRgb(60, 60, 60)));

            CombinedModel = modelGroup;
        }

        private async Task ExecuteLoadModelsCommand()
        {
            if (IsLoaded) return;
            try
            {
                // Load models from the repository
                var groupKey = _configuration["AppSettings:ModelKey"] ?? throw new ArgumentNullException("AppSettings:ModelKey");
                var modelList = await _model3DDataRepository.GetModelAsync(groupKey);
                // 定义要加载的模型
                var list = modelList
                    .Select(model => _model3DLoaderService.LoadModelAsync(model)).ToList();
                var loadedModels = await Task.WhenAll(list);
                // 确保在 UI 线程上执行
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Models.Clear();
                    foreach (var model in loadedModels)
                    {
                        Models.Add(model);
                    }
                    CombineModels();
                    IsLoaded = true;
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("import model 3d error");
            }
        }

        #endregion

    }
}
