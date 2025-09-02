using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace MCCS.Components
{
    /// <summary>
    /// TreeComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class TreeComboBox
    {
        public static readonly DependencyProperty TreeItemsProperty =
            DependencyProperty.Register("TreeItems", typeof(ObservableCollection<TreeNode>), typeof(TreeComboBox));

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(TreeComboBox));

        public static readonly DependencyProperty ControlWidthProperty =
            DependencyProperty.Register("ControlWidth", typeof(double), typeof(TreeComboBox),
                new PropertyMetadata(200.0));

        public static readonly DependencyProperty ControlHeightProperty =
            DependencyProperty.Register("ControlHeight", typeof(double), typeof(TreeComboBox),
                new PropertyMetadata(30.0));

        public static readonly DependencyProperty DropDownMaxHeightProperty =
            DependencyProperty.Register("DropDownMaxHeight", typeof(double), typeof(TreeComboBox),
                new PropertyMetadata(200.0));

        private bool _isDropDownOpen;
        private string _selectedText = "请选择...";

        public TreeComboBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ObservableCollection<TreeNode> TreeItems
        {
            get => (ObservableCollection<TreeNode>)GetValue(TreeItemsProperty);
            set => SetValue(TreeItemsProperty, value);
        }

        public object SelectedValue
        {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        public double ControlWidth
        {
            get => (double)GetValue(ControlWidthProperty);
            set => SetValue(ControlWidthProperty, value);
        }

        public double ControlHeight
        {
            get => (double)GetValue(ControlHeightProperty);
            set => SetValue(ControlHeightProperty, value);
        }

        public double DropDownMaxHeight
        {
            get => (double)GetValue(DropDownMaxHeightProperty);
            set => SetValue(DropDownMaxHeightProperty, value);
        }

        public bool IsDropDownOpen
        {
            get { return _isDropDownOpen; }
            set
            {
                _isDropDownOpen = value;
                OnPropertyChanged(nameof(IsDropDownOpen));
            }
        }

        public string SelectedText
        {
            get => _selectedText;
            set
            {
                _selectedText = value;
                OnPropertyChanged(nameof(SelectedText));
            }
        }

        // 选择改变事件
        public event EventHandler<TreeNodeEventArgs> SelectionChanged;

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            // 下拉框打开时的逻辑
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            // 下拉框关闭时的逻辑
        }

        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = sender as TreeViewItem;
            if (treeViewItem?.Header is TextBlock { Tag: TreeNode node })
            {
                SelectedText = GetNodePath(node);
                SelectedValue = node.Value;
                IsDropDownOpen = false;

                // 触发选择改变事件
                SelectionChanged?.Invoke(this, new TreeNodeEventArgs(node));
            }
            e.Handled = true;
        }

        private string GetNodePath(TreeNode node)
        {
            var path = node.Name;
            var parent = FindParentNode(TreeItems, node);

            while (parent != null)
            {
                path = parent.Name + " > " + path;
                parent = FindParentNode(TreeItems, parent);
            }

            return path;
        }

        private TreeNode FindParentNode(ObservableCollection<TreeNode> items, TreeNode target)
        {
            if (items == null) return null;

            foreach (var item in items)
            {
                if (item.Children?.Contains(target) == true)
                    return item;

                var parent = FindParentNode(item.Children, target);
                if (parent != null)
                    return parent;
            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 树节点数据模型
    public class TreeNode
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; }

        public TreeNode()
        {
            Children = [];
        }

        public TreeNode(string name, object value = null) : this()
        {
            Name = name;
            Value = value ?? name;
        }
    }

    // 事件参数
    public class TreeNodeEventArgs : EventArgs
    {
        public TreeNode SelectedNode { get; }

        public TreeNodeEventArgs(TreeNode node)
        {
            SelectedNode = node;
        }
    }
}
