using System.Collections.ObjectModel;

using MCCS.Infrastructure.Repositories;
using MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters;

using Newtonsoft.Json;

namespace MCCS.Interface.Components.ViewModels.Parameters
{
    /// <summary>
    /// 控制操作参数设置页面 ViewModel
    /// 支持4个控制通道的选择，每个通道可以选择多种控制方式
    /// </summary>
    public sealed class SetControlOperationParamPageViewModel : BaseParameterSetViewModel<List<ControlOperationParamModel>>
    {
        private readonly IStationSiteAggregateRepository _siteAggregateRepository;
        private const int MaxChannelCount = 4;

        public SetControlOperationParamPageViewModel(
            IStationSiteAggregateRepository siteAggregateRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _siteAggregateRepository = siteAggregateRepository;
        }

        #region Properties

        /// <summary>
        /// 可选的控制通道列表
        /// </summary>
        public ObservableCollection<ControlChannelItem> AvailableControlChannels { get; } = [];

        /// <summary>
        /// 四个通道的设置项
        /// </summary>
        public ObservableCollection<ControlChannelSettingItemViewModel> ChannelSettings { get; } = [];

        #endregion

        #region Methods

        protected override async Task ExecuteLoad()
        {
            AvailableControlChannels.Clear();
            ChannelSettings.Clear();

            // 获取当前站点信息
            var stationSite = await _siteAggregateRepository.GetCurrentStationSiteAggregateAsync();
            if (stationSite is null)
            {
                throw new InvalidOperationException("Current station site aggregate is null.");
            }

            // 添加"无"选项
            AvailableControlChannels.Add(new ControlChannelItem
            {
                Id = 0,
                Name = "None",
                DisplayName = "无",
                ChannelId = string.Empty
            });

            // 加载所有控制通道
            foreach (var channelInfo in stationSite.ControlChannelSignalInfos)
            {
                var channel = channelInfo.ControlChannelInfo;
                AvailableControlChannels.Add(new ControlChannelItem
                {
                    Id = channel.Id,
                    Name = channel.ChannelName,
                    DisplayName = channel.ChannelName,
                    ChannelId = channel.ChannelId
                });
            }

            // 初始化四个通道设置项
            for (int i = 1; i <= MaxChannelCount; i++)
            {
                var settingItem = new ControlChannelSettingItemViewModel
                {
                    ChannelIndex = i
                };

                // 如果有已保存的参数，则加载
                if (Parameter != null)
                {
                    var savedParam = Parameter.FirstOrDefault(p => p.ChannelIndex == i);
                    if (savedParam != null)
                    {
                        settingItem.SelectedChannel = AvailableControlChannels
                            .FirstOrDefault(c => c.Id == savedParam.ControlChannelId);
                        settingItem.SetSelectedControlModes(savedParam.AllowedControlModes);
                    }
                }

                // 如果没有选中的通道，默认选择"无"
                settingItem.SelectedChannel ??= AvailableControlChannels.FirstOrDefault();

                ChannelSettings.Add(settingItem);
            }
        }

        protected override string GetParameterJson()
        {
            var parameters = ChannelSettings
                .Where(s => s.SelectedChannel != null && s.SelectedChannel.Id != 0)
                .Select(s => new ControlOperationParamModel
                {
                    ChannelIndex = s.ChannelIndex,
                    ControlChannelId = s.SelectedChannel!.Id,
                    ControlChannelName = s.SelectedChannel.Name,
                    AllowedControlModes = s.GetSelectedControlModes()
                })
                .ToList();

            return JsonConvert.SerializeObject(parameters);
        }

        #endregion
    }
}
