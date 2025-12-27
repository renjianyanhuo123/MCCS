using System.Windows;
using System.Windows.Controls;

namespace MCCS.Controls.Controls.Navigation
{
    public class CurvedTabControl : TabControl
    {
        static CurvedTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CurvedTabControl),
                new FrameworkPropertyMetadata(typeof(CurvedTabControl)));
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CurvedTabItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CurvedTabItem();
        }
    }
}
