using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MCCS.UserControl.Transfer;

/// <summary>
/// TransferUserControl.xaml 的交互逻辑
/// </summary>
public partial class TransferUserControl
{
    public TransferUserControl()
    { 
        InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (SourceItems != null)
            SourceItems.CollectionChanged += OnSourceCollectionChanged;
        if (TargetItems != null)
            TargetItems.CollectionChanged += OnTargetCollectionChanged;
    }
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (SourceItems != null)
            SourceItems.CollectionChanged -= OnSourceCollectionChanged;
        if (TargetItems != null)
            TargetItems.CollectionChanged -= OnTargetCollectionChanged;
    }
    #region 基本依赖属性
    public static readonly DependencyProperty SourceNameDependencyProperty = DependencyProperty.Register(
        nameof(SourceName), 
        typeof(string), 
        typeof(TransferUserControl), 
        new PropertyMetadata(string.Empty, OnSourceNameChanged));

    public static readonly DependencyProperty TargetNameDependencyProperty = DependencyProperty.Register(
        nameof(TargetName),
        typeof(string),
        typeof(TransferUserControl),
        new PropertyMetadata(string.Empty, OnTargetNameChanged));
    #endregion

    #region 集合依赖属性
    public static readonly DependencyProperty SourceItemsDependencyProperty = DependencyProperty.Register(
        nameof(SourceItems),
        typeof(ObservableCollection<TransferItemModel>),
        typeof(TransferUserControl),
        new PropertyMetadata(null, OnSourceItemsChanged));

    public static readonly DependencyProperty TargetItemsDependencyProperty = DependencyProperty.Register(
        nameof(TargetItems),
        typeof(ObservableCollection<TransferItemModel>),
        typeof(TransferUserControl),
        new PropertyMetadata(null, OnTargetItemsChanged)); 
    #endregion

    #region 带值强制转换的依赖属性
    /// <summary>
    /// 左侧源名称依赖属性
    /// </summary>
    public string SourceName
    {
        get => (string)GetValue(SourceNameDependencyProperty);
        set => SetValue(SourceNameDependencyProperty, value);
    }
    /// <summary>
    /// 右侧目标名称依赖属性
    /// </summary>
    public string TargetName
    {
        get => (string)GetValue(TargetNameDependencyProperty);
        set => SetValue(TargetNameDependencyProperty, value);
    }
    /// <summary>
    /// 源数据集合
    /// </summary>
    public ObservableCollection<TransferItemModel> SourceItems
    {
        get => (ObservableCollection<TransferItemModel>)GetValue(SourceItemsDependencyProperty);
        set => SetValue(SourceItemsDependencyProperty, value);
    }

    /// <summary>
    /// 目标数据集合
    /// </summary>
    public ObservableCollection<TransferItemModel> TargetItems
    {
        get => (ObservableCollection<TransferItemModel>)GetValue(TargetItemsDependencyProperty);
        set => SetValue(TargetItemsDependencyProperty, value);
    } 
    #endregion

    #region Event Update
    private static void OnSourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransferUserControl control)
        {
            control.SourceHeader.Text = e.NewValue as string;
        }
        // control.SourceGroupBox = e.NewValue;
    }

    private static void OnTargetNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransferUserControl control)
        {
            control.TargetHeader.Text = e.NewValue as string;
        }
        //control.TargetGroupName.Header = e.NewValue; 
    }
    private static void OnSourceItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransferUserControl control)
        {
            // 取消订阅旧集合的事件
            if (e.OldValue is ObservableCollection<TransferItemModel> oldCollection)
            {
                oldCollection.CollectionChanged -= control.OnSourceCollectionChanged;
            }

            // 订阅新集合的事件
            if (e.NewValue is ObservableCollection<TransferItemModel> newCollection)
            {
                newCollection.CollectionChanged += control.OnSourceCollectionChanged;
                control.SourceList.ItemsSource = newCollection;
            }

            control.UpdateListBox();
        }
    }

    private static void OnTargetItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransferUserControl control)
        {
            // 取消订阅旧集合的事件
            if (e.OldValue is ObservableCollection<TransferItemModel> oldCollection)
            {
                oldCollection.CollectionChanged -= control.OnTargetCollectionChanged;
            }

            // 订阅新集合的事件
            if (e.NewValue is ObservableCollection<TransferItemModel> newCollection)
            {
                newCollection.CollectionChanged += control.OnTargetCollectionChanged;
                control.TargetListBox.ItemsSource = newCollection;
            }

            control.UpdateListBox();
        }
    }
    #endregion

    #region private method
    private void UpdateSelectAllCheckStatus()
    {
        if (!SourceItems.Any(c => c.IsSelected))
        {
            SourceSelectAllCheckBox.IsChecked = false;
            LeftToRight.Background = new SolidColorBrush(Colors.White);
        }
        else if (SourceItems.Count(c => c.IsSelected) == SourceItems.Count)
        {
            SourceSelectAllCheckBox.IsChecked = true;
            LeftToRight.Background = new SolidColorBrush(Color.FromRgb(22, 119, 255));
        }
        else
        {
            SourceSelectAllCheckBox.IsChecked = null;
            LeftToRight.Background = new SolidColorBrush(Color.FromRgb(22, 119, 255));
        }
        if (TargetItems.Count(c => c.IsSelected) == 0)
        {
            TargetSelectAllCheckBox.IsChecked = false;
            RightToLeft.Background = new SolidColorBrush(Colors.White);
        }
        else if (TargetItems.Count(c => c.IsSelected) == TargetItems.Count)
        {
            TargetSelectAllCheckBox.IsChecked = true;
            RightToLeft.Background = new SolidColorBrush(Color.FromRgb(22, 119, 255));
        }
        else
        {
            TargetSelectAllCheckBox.IsChecked = null;
            RightToLeft.Background = new SolidColorBrush(Color.FromRgb(22, 119, 255));
        }
    }

    private void UpdateListBox()
    {
        if (SourceItems == null || TargetItems == null) return;
        if (SourceItems.Count == 0)
        {
            SourceList.Visibility = Visibility.Hidden;
            SourceNodataImage.Visibility = Visibility.Visible;
        }
        else
        {
            SourceList.Visibility = Visibility.Visible;
            SourceNodataImage.Visibility = Visibility.Hidden;
        } 
        if (TargetItems.Count == 0)
        {
            TargetListBox.Visibility = Visibility.Hidden;
            TargetNodataImage.Visibility = Visibility.Visible;
        }
        else
        {
            TargetListBox.Visibility = Visibility.Visible;
            TargetNodataImage.Visibility = Visibility.Hidden;
        }

        UpdateSelectAllCheckStatus();
    }
    // 添加集合变更事件处理方法
    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateListBox();
    }

    private void OnTargetCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateListBox();
    }
    #endregion

    #region Event  
    private void SourceListBox_Clicked(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        var id = checkBox?.Tag as string;
        var operation = SourceItems.FirstOrDefault(c => c.Id == id);
        if (operation != null)
        {
            operation.IsSelected = checkBox?.IsChecked ?? false;
            UpdateSelectAllCheckStatus();
        }
    }
    private void TargetListBox_Clicked(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        var id = checkBox?.Tag as string;
        var operation = TargetItems.FirstOrDefault(c => c.Id == id);
        if (operation != null)
        {
            operation.IsSelected =  checkBox?.IsChecked ?? false;
            UpdateSelectAllCheckStatus();
        }
    } 
    private void SourceSelectAllCheckBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (SourceItems == null || sender is not CheckBox cb) return;
        cb.IsChecked = !cb.IsChecked;
        foreach (var sourceItem in SourceItems)
        {
            if (cb.IsChecked == null) sourceItem.IsSelected = true;
            else
            {
                sourceItem.IsSelected = (bool)cb.IsChecked!;
            }
        } 
        UpdateSelectAllCheckStatus();
        e.Handled = true;
    }
    private void TargetSelectAllCheckBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (TargetItems == null || sender is not CheckBox cb) return;
        cb.IsChecked = !cb.IsChecked;
        foreach (var targetItem in TargetItems)
        {
            targetItem.IsSelected = (bool)cb.IsChecked!;
        } 
        UpdateSelectAllCheckStatus();
        e.Handled = true;
    }
    private void RightToLeft_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!TargetItems.Any(c => c.IsSelected)) return;
        var selectedItems = TargetItems.Where(x => x.IsSelected).ToList();
        foreach (var item in selectedItems)
        {
            SourceItems.Add(item);
            TargetItems.Remove(item); 
        }
        UpdateListBox();
        UpdateSelectAllCheckStatus();
    }
    private void LeftToRight_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!SourceItems.Any(c => c.IsSelected)) return;
        var selectedItems = SourceItems.Where(x => x.IsSelected).ToList();
        foreach (var item in selectedItems)
        {
            TargetItems.Add(item);
            SourceItems.Remove(item);
        }
        UpdateListBox();
        UpdateSelectAllCheckStatus();
    }
    #endregion 
}