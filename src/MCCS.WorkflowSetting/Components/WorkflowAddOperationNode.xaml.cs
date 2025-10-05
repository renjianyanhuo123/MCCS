using System.Windows;
using System.Windows.Input;

namespace MCCS.WorkflowSetting.Components
{
    /// <summary>
    /// WorkflowAddOperationNode.xaml 的交互逻辑
    /// </summary>
    public partial class WorkflowAddOperationNode
    {
        public WorkflowAddOperationNode()
        {
            InitializeComponent();
        }

        #region Command Event
        public static readonly DependencyProperty AddOpCommandProperty =
            DependencyProperty.Register(
                nameof(AddOpCommand),
                typeof(ICommand),
                typeof(WorkflowAddOperationNode));

        public ICommand AddOpCommand
        {
            get => (ICommand)GetValue(AddOpCommandProperty);
            set => SetValue(AddOpCommandProperty, value);
        }
        #endregion

        private void OnMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            AddOpCommand?.Execute(null);
        }
    }
}
