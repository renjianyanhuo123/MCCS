using System.Windows.Media;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class StepNode : BaseNode
    {
        private string _soureId;
        private readonly IEventAggregator _eventAggregator;

        public StepNode(string sourceId, IEventAggregator eventAggregator)
        {
            _soureId = sourceId;
            _eventAggregator = eventAggregator;
            OperationNodeClickedCommand = new DelegateCommand(ExecuteOperationNodeClickedCommand);
            DeleteNodeCommand = new DelegateCommand(ExecuteDeleteNodeCommand);
            CancelCommand = new DelegateCommand(ExecuteCancelCommand);
            ConfigueDeleteCommand = new DelegateCommand(ExecuteConfigueDeleteCommand);
            ModifityTitleCommand = new DelegateCommand(ExecuteModifityTitleCommand);
            TitleLostFocusCommand = new DelegateCommand(ExecuteTitleLostFocusCommand);
            TitleBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9955")); 
        }
        #region Property
        /// <summary>
        /// 节点标题
        /// </summary>
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private bool _titleIsEditable = false;
        public bool TitleIsEditable
        {
            get => _titleIsEditable;
            set => SetProperty(ref _titleIsEditable, value);
        }

        /// <summary>
        /// 节点背景色
        /// </summary>
        private Brush _titleBackground = Brushes.Transparent;
        public Brush TitleBackground
        {
            get => _titleBackground;
            set => SetProperty(ref _titleBackground, value);
        }

        private bool _isOpen = false;
        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        private bool _isShowShade = false;
        public bool IsShowShade
        {
            get => _isShowShade;
            set => SetProperty(ref _isShowShade, value);
        }
        #endregion

        #region Command
        public DelegateCommand OperationNodeClickedCommand { get; } 
        public DelegateCommand DeleteNodeCommand { get; } 
        public DelegateCommand CancelCommand { get; } 
        public DelegateCommand ConfigueDeleteCommand { get; }
        public DelegateCommand ModifityTitleCommand { get; }
        public DelegateCommand TitleLostFocusCommand { get; }

        #endregion
        private void ExecuteTitleLostFocusCommand()
        {
            TitleIsEditable = false;
        }

        private void ExecuteModifityTitleCommand()
        {
            TitleIsEditable = true;
            IsOpen = false;
        }

        private void ExecuteConfigueDeleteCommand() =>
            _eventAggregator.GetEvent<DeleteNodeEvent>()
                .Publish(new DeleteNodeEventParam
                {
                    Source = Parent?.Id ?? "",
                    NodeId = Id
                });

        private void ExecuteOperationNodeClickedCommand() => IsOpen = true;

        private void ExecuteDeleteNodeCommand()
        {
            IsShowShade = true;
            IsOpen = false;
        }

        private void ExecuteCancelCommand() => IsShowShade = false;
    }
}
