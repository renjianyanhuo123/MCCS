using MCCS.Common.DataManagers;
using MCCS.Common.DataManagers.CurrentTest;
using MCCS.Common.Resources.ViewModels;
using MCCS.Components.LayoutRootComponents;
using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;
using MCCS.Infrastructure.Repositories.Method;

using Newtonsoft.Json;

namespace MCCS.ViewModels.ProjectManager
{
    public class ProjectOperationPageViewModel : BaseViewModel
    {
        private readonly IMethodRepository _methodRepository;
        private readonly ILayoutTreeTraversal _layoutTreeTraversal;
        private long _methodId = -1;

        public ProjectOperationPageViewModel(
            IMethodRepository methodRepository,
            ILayoutTreeTraversal layoutTreeTraversal,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _methodRepository = methodRepository;
            _layoutTreeTraversal = layoutTreeTraversal;
            LoadCommand = new AsyncDelegateCommand(ExecuteLoadCommand);
            PauseAndContinueTestCommand = new AsyncDelegateCommand(ExecutePauseAndContinueTestCommand);
            StartAndStopTestCommand = new AsyncDelegateCommand(ExecuteStartAndStopTestCommand);
            GlobalDataManager.Instance.SetValue(new CurrentTestInfo());
            GlobalDataManager.Instance.CurrentTestInfo.StartedEvent += OnStartedEvent;
            GlobalDataManager.Instance.CurrentTestInfo.StoppedEvent += OnStopedEvent;
            GlobalDataManager.Instance.CurrentTestInfo.PausedEvent += OnPausedEvent;
            GlobalDataManager.Instance.CurrentTestInfo.ContinuedEvent += OnContinuedEvent;
        }

        #region Property
        private LayoutRootViewModel? _layoutRootViewModel;
        public LayoutRootViewModel? LayoutRootViewModel
        {
            get => _layoutRootViewModel;
            set => SetProperty(ref _layoutRootViewModel, value);
        }

        private bool _isStartedTest;
        public bool IsStartedTest
        {
            get => _isStartedTest;
            set => SetProperty(ref _isStartedTest, value);
        }

        private bool _isPaused;
        public bool IsPaused 
        { 
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }
        #endregion

        #region Command 
        public AsyncDelegateCommand LoadCommand { get; } 

        public AsyncDelegateCommand StartAndStopTestCommand { get; } 

        public AsyncDelegateCommand PauseAndContinueTestCommand { get; }
        #endregion

        public override void OnNavigatedTo(NavigationContext navigationContext) => _methodId = navigationContext.Parameters.GetValue<long>("MethodId");

        #region Private Method
        private async Task ExecuteLoadCommand()
        {
            if (_methodId == -1) throw new ArgumentNullException(nameof(_methodId));
            var interfaceSettingModel = await _methodRepository.GetInterfaceSettingAsync(_methodId);
            if (interfaceSettingModel == null) throw new ArgumentException(nameof(interfaceSettingModel));
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new BaseNodeJsonConverter()
                }
            };
            var nodes = interfaceSettingModel?.RootSetting == null ? [] : JsonConvert.DeserializeObject<List<BaseNode>>(interfaceSettingModel.RootSetting, settings);
            var rootNode = _layoutTreeTraversal.BuildRootNode(CellTypeEnum.DisplayOnly, nodes ?? []);
            LayoutRootViewModel = new LayoutRootViewModel(rootNode, _eventAggregator);
        }

        private async Task ExecutePauseAndContinueTestCommand() 
        {
            if (IsPaused)
            {
                // 暂停测试
            }
            else
            {
                // 继续测试
            }
        }

        private async Task ExecuteStartAndStopTestCommand() 
        {
            if (IsStartedTest)
            {
                
            }
            else
            {
                
            }
        }

        private bool OnStopedEvent()
        {
            return true;
        }

        private bool OnStartedEvent()
        {
            return true;
        }

        private bool OnPausedEvent()
        {
            return true;
        }

        private bool OnContinuedEvent()
        {
            return true;
        }
        #endregion
    }
}
