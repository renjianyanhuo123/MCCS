using MCCS.Common.Resources.ViewModels;
using MCCS.Components.LayoutRootComponents;
using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Infrastructure.Models.MethodManager;
using MCCS.Infrastructure.Models.MethodManager.InterfaceNodes;
using MCCS.Infrastructure.Repositories.Method;

using Newtonsoft.Json;

namespace MCCS.ViewModels.MethodManager.Contents
{
    public sealed class MethodInterfaceSettingPageViewModel : BaseViewModel
    { 
        private long _methodId = -1;
        private readonly IMethodRepository _methodRepository;
        private readonly ILayoutTreeTraversal _layoutTreeTraversal;

        public MethodInterfaceSettingPageViewModel(
            ILayoutTreeTraversal layoutTreeTraversal,
            IMethodRepository methodRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _layoutTreeTraversal = layoutTreeTraversal;
            _methodRepository = methodRepository;
            LoadCommand = new AsyncDelegateCommand(ExecuteCommand);
        }
        //public override bool IsNavigationTarget(NavigationContext navigationContext) => false;//

        public override void OnNavigatedTo(NavigationContext navigationContext) => _methodId = navigationContext.Parameters.GetValue<long>("MethodId");

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (LayoutRootViewModel?.RootNode == null) return;
            // 后序遍历布局树,存储到数据库中
            var list = _layoutTreeTraversal.PostOrderToBaseNodes(LayoutRootViewModel.RootNode);
            var jsonStr = JsonConvert.SerializeObject(list);
            _methodRepository.AddInterfaceSetting(new MethodInterfaceSettingModel
            {
                MethodId = _methodId,
                RootSetting = jsonStr
            });
        }

        #region Private Method  
        private async Task ExecuteCommand()
        {
            if (_methodId == -1) throw new ArgumentNullException(nameof(_methodId));
            var settingModel = await _methodRepository.GetInterfaceSettingAsync(_methodId); 
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new BaseNodeJsonConverter()
                }
            };
            var nodes = settingModel?.RootSetting == null ? [] : JsonConvert.DeserializeObject<List<BaseNode>>(settingModel.RootSetting, settings);  
            var rootNode = _layoutTreeTraversal.BuildRootNode(CellTypeEnum.Editable, nodes ?? []);
            LayoutRootViewModel = new LayoutRootViewModel(rootNode, _eventAggregator);
        }
        #endregion

        #region Command
        public AsyncDelegateCommand LoadCommand { get; }
        #endregion

        #region Property 
        private LayoutRootViewModel? _layoutRootViewModel;
        public LayoutRootViewModel? LayoutRootViewModel
        {
            get => _layoutRootViewModel;
            set => SetProperty(ref _layoutRootViewModel, value);
        }
        #endregion 
    }
}
