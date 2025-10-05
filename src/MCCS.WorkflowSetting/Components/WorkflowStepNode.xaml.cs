using System.Windows;
using System.Windows.Media;

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
        }

        #region 依赖属性
        public Brush TitleBackground
        {
            get => (Brush)this.GetValue(TitleBackgroundProperty);
            set => this.SetValue(TitleBackgroundProperty, (object)value);
        }

        public static readonly DependencyProperty TitleBackgroundProperty =
            DependencyProperty.Register(nameof(TitleBackground),
                typeof(Brush),
                typeof(WorkflowStepNode),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9955"))));
        #endregion
    }
}
