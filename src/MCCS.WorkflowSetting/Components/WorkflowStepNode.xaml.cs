using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MCCS.WorkflowSetting.Components.ViewModels;

namespace MCCS.WorkflowSetting.Components
{
    /// <summary>
    /// WorkflowStepNode.xaml 的交互逻辑
    /// </summary>
    public partial class WorkflowStepNode
    {
        public WorkflowStepNode()
        {
            InitializeComponent();
            DataContext = new WorkflowStepNodeViewModel();
        }

        #region 依赖属性
        public Brush TitleBackground
        {
            get => (Brush)GetValue(TitleBackgroundProperty);
            set => SetValue(TitleBackgroundProperty, value);
        }

        public static readonly DependencyProperty TitleBackgroundProperty =
            DependencyProperty.Register(nameof(TitleBackground),
                typeof(Brush),
                typeof(WorkflowStepNode),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9955"))));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title),
                typeof(string),
                typeof(WorkflowStepNode),
                new PropertyMetadata(string.Empty));
        #endregion  
    }
}
