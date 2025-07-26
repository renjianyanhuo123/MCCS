using MCCS.Components.GlobalNotification.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MCCS.Components.GlobalNotification
{
    /// <summary>
    /// NotificationItemControl.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationItemControl : UserControl
    {
        public NotificationItemControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        public event RoutedEventHandler? CloseClicked; 

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is Models.NotificationItem notification)
            {
                SetStyle(notification.Type);
            }
        }

        private void SetStyle(NotificationType type)
        {
            var (bgColor, borderColor, iconColor, iconText) = type switch
            {
                NotificationType.Success => ("#E8F5E8", "#4CAF50", "#4CAF50", "✓"),
                NotificationType.Warning => ("#FFF3E0", "#FF9800", "#FF9800", "!"),
                NotificationType.Error => ("#FFEBEE", "#F44336", "#F44336", "×"),
                _ => ("#E3F2FD", "#2196F3", "#2196F3", "i")
            };

            MainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor));
            MainBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderColor));
            IconEllipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iconColor));
            IconText.Text = iconText;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseClicked?.Invoke(this, e);
        }
    }
}
