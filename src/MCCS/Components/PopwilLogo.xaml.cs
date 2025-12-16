using System.Windows;
using System.Windows.Media;

namespace MCCS.Components
{
    /// <summary>
    /// PopwilLogo.xaml 的交互逻辑
    /// </summary>
    public partial class PopwilLogo
    {
        public static readonly DependencyProperty LogoColorProperty =
            DependencyProperty.Register(
                nameof(LogoColor),
                typeof(Brush),
                typeof(PopwilLogo),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(227, 29, 43))));

        /// <summary>
        /// Logo的颜色，默认为 #E31D2B
        /// </summary>
        public Brush LogoColor
        {
            get => (Brush)GetValue(LogoColorProperty);
            set => SetValue(LogoColorProperty, value);
        }


        public PopwilLogo()
        {
            InitializeComponent();
        }
    }
}
