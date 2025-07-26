using System.Windows.Controls;
using System.Windows;

namespace MCCS.Behaviors
{
    public static class TreeViewSelectedItemBehavior
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItem",
                typeof(object),
                typeof(TreeViewSelectedItemBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static object GetSelectedItem(DependencyObject obj)
        {
            return obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TreeView treeView) return;
            treeView.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;

            if (e.NewValue != null)
            {
                SetSelectedTreeViewItem(treeView, e.NewValue);
            }
        }

        private static void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView treeView)
            {
                SetSelectedItem(treeView, e.NewValue);
            }
        }

        private static void SetSelectedTreeViewItem(TreeView treeView, object item)
        {
            if (treeView.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem container)
            {
                container.IsSelected = true;
                container.BringIntoView();
                return;
            }

            // 如果没有找到，可能需要遍历所有容器
            foreach (var rootItem in treeView.Items)
            {
                var rootContainer = treeView.ItemContainerGenerator.ContainerFromItem(rootItem) as TreeViewItem;
                if (rootContainer != null && FindAndSelectItem(rootContainer, item))
                {
                    break;
                }
            }
        }

        private static bool FindAndSelectItem(TreeViewItem? container, object item)
        {
            if (container == null) return false;

            if (container.DataContext == item)
            {
                container.IsSelected = true;
                container.BringIntoView();
                return true;
            }

            // 展开容器以确保子项被生成
            container.IsExpanded = true;
            container.UpdateLayout();

            for (var i = 0; i < container.Items.Count; i++)
            {
                var childContainer = container.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (FindAndSelectItem(childContainer, item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
