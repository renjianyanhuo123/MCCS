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
        }

        #region Property
        private LayoutRootViewModel? _layoutRootViewModel;
        public LayoutRootViewModel? LayoutRootViewModel
        {
            get => _layoutRootViewModel;
            set => SetProperty(ref _layoutRootViewModel, value);
        }
        #endregion

        #region Command 
        public AsyncDelegateCommand LoadCommand { get; } 
        #endregion

        public override void OnNavigatedTo(NavigationContext navigationContext) => _methodId = navigationContext.Parameters.GetValue<long>("MethodId");

        public async Task ExecuteLoadCommand()
        {
            if (_methodId == -1) throw new ArgumentNullException(nameof(_methodId));
            var interfaceSettingModel = await _methodRepository.GetInterfaceSettingAsync(_methodId);
            if (interfaceSettingModel == null) throw new ArgumentException(nameof(interfaceSettingModel)); 
            var components = await _methodRepository.GetUiComponentsAsync();
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new BaseNodeJsonConverter()
                }
            };
            var nodes = interfaceSettingModel?.RootSetting == null ? [] : JsonConvert.DeserializeObject<List<BaseNode>>(interfaceSettingModel.RootSetting, settings);
            var rootNode = _layoutTreeTraversal.BuildRootNode(CellTypeEnum.DisplayOnly, nodes ?? [], components);
            LayoutRootViewModel = new LayoutRootViewModel(rootNode, _eventAggregator);
        }
    }
}
