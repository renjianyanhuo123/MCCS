using System.Windows.Media;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class StepNode : BaseNode
    {
        private readonly string _soureId;
        private readonly IEventAggregator _eventAggregator;

        public StepNode(string sourceId, IEventAggregator eventAggregator)
        {
            _soureId = sourceId;
            OperationNodeClickedCommand = new DelegateCommand(ExecuteOperationNodeClickedCommand);
            DeleteNodeCommand = new DelegateCommand(ExecuteDeleteNodeCommand);
            CancelCommand = new DelegateCommand(ExecuteCancelCommand);
            ConfigueDeleteCommand = new DelegateCommand(ExecuteConfigueDeleteCommand);
            TitleBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9955"));
            _eventAggregator = eventAggregator;
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
        /// <summary>
        /// 节点背景色
        /// </summary>
        private Brush _titleBackground;
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

        public DelegateCommand OperationNodeClickedCommand { get; }

        public DelegateCommand DeleteNodeCommand { get; }

        public DelegateCommand CancelCommand { get; }

        public DelegateCommand ConfigueDeleteCommand { get; }

        private void ExecuteConfigueDeleteCommand()
        {
            _eventAggregator.GetEvent<DeleteNodeEvent>()
                .Publish(new DeleteNodeEventParam
                {
                    Source = _soureId,
                    NodeId = Id
                });
        }

        private void ExecuteOperationNodeClickedCommand()
        {
            IsOpen = true;
        }

        private void ExecuteDeleteNodeCommand()
        {
            IsShowShade = true;
            IsOpen = false;
        }

        private void ExecuteCancelCommand()
        {
            IsShowShade = false;
        }
    }
}
