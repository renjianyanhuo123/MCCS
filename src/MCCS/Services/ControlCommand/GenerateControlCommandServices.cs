using MCCS.Core.Devices.Manager;
using MCCS.ViewModels.Pages.Controllers;
using MCCS.Views.Pages.Controllers;

namespace MCCS.Services.ControlCommand
{
    public class GenerateControlCommandServices : IGenerateControlCommandServices
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDeviceManager _deviceManager;
        private readonly IRegionManager _regionManager;
        private readonly Dictionary<string, ControllerMainPage> _controllerMainPages = new();

        public GenerateControlCommandServices(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IDeviceManager deviceManager)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _deviceManager = deviceManager;
        }

        public ControllerMainPage CreateControllerPage(string channelId, string channelName)
        {
            // 检查是否已存在该通道的页面
            if (_controllerMainPages.TryGetValue(channelId, out var existingPage))
            {
                return existingPage;
            }

            // 创建新的ViewModel
            var viewModel = new ControllerMainPageViewModel(_eventAggregator);

            // 创建新的View
            var view = new ControllerMainPage
            {
                DataContext = viewModel
            };

            // 初始化ViewModel
            // viewModel.Initialize(channelId, channelName);

            // 缓存页面
            _controllerMainPages[channelId] = view;

            return view;
        }
    }
}
