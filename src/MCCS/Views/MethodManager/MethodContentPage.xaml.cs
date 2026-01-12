using Prism.Regions;

namespace MCCS.Views.MethodManager
{
    /// <summary>
    /// MethodContentPage.xaml 的交互逻辑
    /// </summary>
    public partial class MethodContentPage
    {
        private readonly IRegionManager _regionManager;

        public MethodContentPage(IRegionManager regionManager)
        {
            InitializeComponent();
            _regionManager = regionManager;
            Unloaded += MethodContentPage_Unloaded;
        }

        private void MethodContentPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 当页面卸载时，移除子Region以避免再次导航时重复注册
            if (_regionManager.Regions.ContainsRegionWithName(GlobalConstant.MethodNavigateRegionName))
            {
                _regionManager.Regions.Remove(GlobalConstant.MethodNavigateRegionName);
            }
            Unloaded -= MethodContentPage_Unloaded;
        }
    }
}
