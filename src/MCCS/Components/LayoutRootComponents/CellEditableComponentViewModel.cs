using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Events.Mehtod.DynamicGridOperationEvents;
using MCCS.Infrastructure.Repositories.Method;

using Serilog;

namespace MCCS.Components.LayoutRootComponents
{
    public class CellEditableComponentViewModel : LayoutNode
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMethodRepository _methodRepository;

        public CellEditableComponentViewModel(IEventAggregator eventAggregator, IMethodRepository methodRepository)
        {
            _eventAggregator = eventAggregator;
            _methodRepository = methodRepository;
            CutHorizontalCommand = new DelegateCommand(ExecuteCutHorizontalCommand);
            CutVerticalCommand = new DelegateCommand(ExecuteCutVerticalCommand);
            DeleteNodeCommand = new DelegateCommand(ExecuteDeleteNodeCommand);
            SetNodeCommand = new DelegateCommand(ExecuteSetNodeCommand);
            SetNodeParamterCommand = new DelegateCommand(ExecuteSetNodeParamterCommand);
            _eventAggregator.GetEvent<SelectedComponentEvent>().Subscribe(OnSelectedComponent);
            _eventAggregator.GetEvent<SaveParameterEvent>().Subscribe(param =>
            {
                if (param.SourceId != Id) return;
                ParamterJson = param.Parameter;
            });
        }

        #region Command 
        public DelegateCommand CutHorizontalCommand { get; }
        public DelegateCommand CutVerticalCommand { get; }
        public DelegateCommand DeleteNodeCommand { get; }
        public DelegateCommand SetNodeCommand { get; }
        public DelegateCommand SetNodeParamterCommand { get; } 
        #endregion

        #region Property
        private string _title = "未设置节点";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _icon = "BlockHelper";
        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }
         
        private bool _isCanSetParam; 
        public bool IsCanSetParam
        {
            get => _isCanSetParam;
            set => SetProperty(ref _isCanSetParam, value);
        }
        /// <summary>
        /// 配置的UIComponent节点Id
        /// </summary>
        private long _nodeId = -1;
        public long NodeId
        {
            get => _nodeId;
            set => SetProperty(ref _nodeId, value);
        }

        private string _nodeSettingParamText = ""; 
        public string NodeSettingParamText
        {
            get => _nodeSettingParamText;
            set => SetProperty(ref _nodeSettingParamText, value);
        }

        /// <summary>
        /// 节点参数配置
        /// </summary>
        private string? _paramterJson;
        public string? ParamterJson { get => _paramterJson;
            private set
            {
                _paramterJson = value;
                NodeSettingParamText = "已设置参数";
            }
        }  
        #endregion

        #region Private Method
        private void ExecuteCutHorizontalCommand()
        {
            var parent = Parent;
            var splitter = new SplitterHorizontalLayoutNode(this, new CellEditableComponentViewModel(_eventAggregator, _methodRepository));
            if (parent != null)
            {
                splitter.Parent = parent;
                if (parent.LeftNode == this)
                {
                    parent.LeftNode = splitter; 
                }
                else
                {
                    parent.RightNode = splitter;
                } 
            }
            else
            {
                _eventAggregator.GetEvent<ChangedRootEvent>().Publish(new ChangedRootEventParam
                {
                    Root = splitter
                });
            }  
        } 

        private void ExecuteCutVerticalCommand()
        {
            var parent = Parent;
            var splitter = new SplitterVerticalLayoutNode(this, new CellEditableComponentViewModel(_eventAggregator, _methodRepository)); 
            if (parent != null)
            {
                splitter.Parent = parent;
                if (parent.LeftNode == this)
                { 
                    parent.LeftNode = splitter;
                }
                else
                {
                    parent.RightNode = splitter;
                }
            }
            else
            { 
                _eventAggregator.GetEvent<ChangedRootEvent>().Publish(new ChangedRootEventParam
                {
                    Root = splitter
                });
            }
        }

        private void ExecuteDeleteNodeCommand()
        {
            if (Parent == null) return;
            var grandFather = Parent.Parent;
            var brotherNode = Parent.LeftNode == this ? Parent.RightNode : Parent.LeftNode;
            if (brotherNode == null) return;
            if (grandFather == null)
            {
                brotherNode.Parent = null;
                _eventAggregator.GetEvent<ChangedRootEvent>().Publish(new ChangedRootEventParam
                {
                    Root = brotherNode
                });
            }
            else
            {
                brotherNode.Parent = grandFather;
                if (Parent == grandFather.LeftNode)
                {
                    grandFather.LeftNode = null;
                    grandFather.LeftNode = brotherNode; 
                }
                else
                {
                    grandFather.RightNode = null;
                    grandFather.RightNode = brotherNode;
                }
            }
        }

        private void ExecuteSetNodeCommand() =>
            _eventAggregator.GetEvent<OpenUiCompontsEvent>().Publish(new OpenUiCompontsEventParam
            {
                SourceId = Id
            });

        private void ExecuteSetNodeParamterCommand() =>
            _eventAggregator.GetEvent<OpenParamterSetEvent>().Publish(new OpenParamterSetEventParam
            {
                SourceId = Id,
                Parameter = ParamterJson
            });

        private async void OnSelectedComponent(SelectedComponentEventParam param)
        {
            try
            {
                if (param.NodeId <= 0) return;
                if (param.SourceId != Id) return; 
                var component = await _methodRepository.GetMethodUiComponentByIdAsync(param.NodeId);
                if (component == null) return;
                NodeId = param.NodeId;
                Title = component.Title;
                Icon = component.Icon ?? "";
                NodeSettingParamText = "未设置节点参数";
                IsCanSetParam = component.IsCanSetParam;
            }
            catch (Exception e)
            {
                Log.Error($"选择Ui节点失败:{e.Message}"); 
            }
        }

        #endregion
    }
}
