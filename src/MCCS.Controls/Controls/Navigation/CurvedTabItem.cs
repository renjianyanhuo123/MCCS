using System.Windows;
using System.Windows.Controls;

namespace MCCS.Controls.Controls.Navigation
{
    public class CurvedTabItem : TabItem
    {
        static CurvedTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CurvedTabItem),
                new FrameworkPropertyMetadata(typeof(CurvedTabItem)));
        }

        #region Dependency Properties

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(CurvedTabItem),
                new PropertyMetadata(new CornerRadius(30)));

        #endregion
    }
}
