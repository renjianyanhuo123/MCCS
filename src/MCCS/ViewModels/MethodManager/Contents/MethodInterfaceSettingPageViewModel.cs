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

        public MethodInterfaceSettingPageViewModel(
            IMethodRepository methodRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _methodRepository = methodRepository;
            LoadCommand = new AsyncDelegateCommand(ExecuteCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext) => _methodId = navigationContext.Parameters.GetValue<long>("MethodId");

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (LayoutRootViewModel?.RootNode == null) return;
            // 后序遍历布局树,存储到数据库中
            var list = LayoutTreeTraversal.PostOrderToBaseNodes(LayoutRootViewModel.RootNode);
            var jsonStr = JsonConvert.SerializeObject(list);
            _methodRepository.AddInterfaceSetting(new MethodInterfaceSettingModel
            {
                MethodId = _methodId,
                RootSetting = jsonStr
            });
        }

        #region Private Method 
        private LayoutNode BuildRootNode(List<BaseNode> nodes, List<MethodUiComponentsModel> components)
        {
            if (nodes.Count == 0) return new CellEditableComponentViewModel(_eventAggregator, _methodRepository); 
            LayoutNode? loopNode = null;
            var nodeDic = new Dictionary<string, LayoutNode>();
            //(1)构建二叉树;nodes采用后序遍历存储
            foreach (var node in nodes)
            { 
                switch (node)
                {
                    case CellNode cellNode:
                        loopNode = new CellEditableComponentViewModel(_eventAggregator, _methodRepository, cellNode, components.FirstOrDefault(c => c.Id == cellNode.NodeId)); 
                        break;
                    case SplitterNode splitterNode:
                        if (splitterNode.LeftNodeId == null 
                            || !nodeDic.ContainsKey(splitterNode.LeftNodeId) 
                            || splitterNode.RightNodeId ==null 
                            || !nodeDic.ContainsKey(splitterNode.RightNodeId))
                            throw new InvalidOperationException("节点数据异常，无法构建布局树");
                        var leftNode = nodeDic[splitterNode.LeftNodeId];
                        var rightNode = nodeDic[splitterNode.RightNodeId];
                        if (splitterNode.NodeType == NodeTypeEnum.SplitterHorizontal)
                        {
                            loopNode = new SplitterHorizontalLayoutNode(splitterNode.LeftRatio,
                                splitterNode.RightRatio, splitterNode.SplitterSize, leftNode, rightNode); 
                        }
                        else
                        {
                            loopNode = new SplitterVerticalLayoutNode(splitterNode.LeftRatio,
                                splitterNode.RightRatio, splitterNode.SplitterSize, leftNode, rightNode);
                        }
                        break;
                    default:
                        throw new NotSupportedException("不支持该种类型的节点");
                } 
                nodeDic.Add(node.Id, loopNode);  
            }
            if (loopNode == null) throw new InvalidOperationException("节点数据异常，无法构建布局树");
            return loopNode;
        }

        private async Task ExecuteCommand()
        {
            if (_methodId == -1) throw new ArgumentNullException(nameof(_methodId));
            var settingModel = await _methodRepository.GetInterfaceSettingAsync(_methodId);
            var components = await _methodRepository.GetUiComponentsAsync();
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new BaseNodeJsonConverter()
                }
            };
            var nodes = settingModel?.RootSetting == null ? [] : JsonConvert.DeserializeObject<List<BaseNode>>(settingModel.RootSetting, settings);  
            var rootNode = BuildRootNode(nodes ?? [] , components);
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
