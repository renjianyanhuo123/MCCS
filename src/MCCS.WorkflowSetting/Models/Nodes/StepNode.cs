using System.Windows.Input;
using System.Windows.Media;
using MCCS.Infrastructure;
using MCCS.WorkflowSetting.Components.ViewModels;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.WorkflowSetting.Models.Nodes
{
    public class StepNode : BaseNode
    { 
        public StepNode()
        {
            OperationNodeClickedCommand = new RelayCommand(ExecuteOperationNodeClickedCommand, _ => true);
            DeleteNodeCommand = new RelayCommand(ExecuteDeleteNodeCommand, _ => true);
            CancelCommand = new RelayCommand(ExecuteCancelCommand, _ => true);
            ConfigueDeleteCommand = new RelayCommand(ExecuteConfigueDeleteCommand, _ => true);
            TitleBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9955"));
        }

        public bool IsSettingNode { get; set; } = false;

        /// <summary>
        /// 节点标题
        /// </summary>
        private string _title = string.Empty;
        public string Title { 
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

        public ICommand OperationNodeClickedCommand { get; }

        public ICommand DeleteNodeCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ConfigueDeleteCommand { get; }

        private void ExecuteConfigueDeleteCommand(object? param)
        {
            EventMediator.Instance.Publish(new DeleteNodeEvent
            {
                NodeId = Id
            });
        }

        private void ExecuteOperationNodeClickedCommand(object? param)
        {
            IsOpen = true;
        }

        private void ExecuteDeleteNodeCommand(object? param)
        {
            IsShowShade = true;
            IsOpen = false;
        }

        private void ExecuteCancelCommand(object? param)
        {
            IsShowShade = false;
        }
    }
}
