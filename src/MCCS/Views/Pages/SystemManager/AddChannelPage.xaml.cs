using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MCCS.ViewModels.Pages.SystemManager;

namespace MCCS.Views.Pages.SystemManager
{
    /// <summary>
    /// AddChannelPage.xaml 的交互逻辑
    /// </summary>
    public partial class AddChannelPage : UserControl
    {
        public AddChannelPage()
        {
            InitializeComponent();
            // 设置MessageQueue
            var viewModel = (AddChannelPageViewModel)this.DataContext;
            viewModel.MessageQueue = SnackbarSeven.MessageQueue;
        }
    }
}
