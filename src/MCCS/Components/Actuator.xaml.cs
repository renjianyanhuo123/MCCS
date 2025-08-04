using System.Windows;

namespace MCCS.Components
{
    /// <summary>
    /// Actuator.xaml 的交互逻辑
    /// </summary>
    public partial class Actuator
    {
        public Actuator()
        {
            InitializeComponent();
        }

        // 依赖属性：位移
        public static readonly DependencyProperty DisplacementProperty =
            DependencyProperty.Register("Displacement", typeof(double), typeof(Actuator), new PropertyMetadata(0.0));
        // 依赖属性：力
        public static readonly DependencyProperty ForceProperty =
            DependencyProperty.Register("Force", typeof(double), typeof(Actuator), new PropertyMetadata(0.0));

        // 位移属性封装
        public double Displacement
        {
            get => (double)GetValue(DisplacementProperty);
            set => SetValue(DisplacementProperty, value);
        }
        // 力属性封装
        public double Force
        {
            get => (double)GetValue(ForceProperty);
            set => SetValue(ForceProperty, value);
        }
    }
}
