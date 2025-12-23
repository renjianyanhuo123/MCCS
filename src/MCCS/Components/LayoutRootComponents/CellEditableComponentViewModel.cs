using System.Windows.Media;

using MCCS.Components.LayoutRootComponents.ViewModels;
using MCCS.Events.Mehtod.DynamicGridOperationEvents; 

namespace MCCS.Components.LayoutRootComponents
{
    public class CellEditableComponentViewModel : LayoutNode
    {
        private readonly IEventAggregator _eventAggregator; 

        public CellEditableComponentViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            CutHorizontalCommand = new DelegateCommand(ExecuteCutHorizontalCommand);
            CutVerticalCommand = new DelegateCommand(ExecuteCutVerticalCommand);
            DeleteNodeCommand = new DelegateCommand(ExecuteDeleteNodeCommand);
        }

        #region Command 
        public DelegateCommand CutHorizontalCommand { get; }

        public DelegateCommand CutVerticalCommand { get; }

        public DelegateCommand DeleteNodeCommand { get; }
        #endregion

        #region Property
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private ImageSource _icon;
        public ImageSource Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        private string _viewTypeName;
        public string ViewTypeName
        {
            get => _viewTypeName;
            set => SetProperty(ref _viewTypeName, value);
        }

        private bool _isCanSetParam; 
        public bool IsCanSetParam
        {
            get => _isCanSetParam;
            set => SetProperty(ref _isCanSetParam, value);
        }

        private long _nodeId;
        public long NodeId
        {
            get => _nodeId;
            set => SetProperty(ref _nodeId, value);
        }

        private string _nodeSettingParamText; 
        public string NodeSettingParamText
        {
            get => _nodeSettingParamText;
            set => SetProperty(ref _nodeSettingParamText, value);
        }
         
        public long NodeSettingParamId { get; private set; } 
        #endregion

        #region Private Method
        private void ExecuteCutHorizontalCommand()
        {
            var parent = Parent;
            var splitter = new SplitterHorizontalLayoutNode(this, new CellEditableComponentViewModel(_eventAggregator));
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
            var splitter = new SplitterVerticalLayoutNode(this, new CellEditableComponentViewModel(_eventAggregator)); 
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
        #endregion
    }
}
