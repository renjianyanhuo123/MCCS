using System.Collections.ObjectModel;

using MCCS.Infrastructure.Helper;
using MCCS.Infrastructure.Repositories;
using MCCS.Interface.Components.Enums;
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

        public SetControlOperationParamPageViewModel(
            IStationSiteAggregateRepository siteAggregateRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _siteAggregateRepository = siteAggregateRepository;
        }

        #region Properties 
        /// <summary>
        /// 所有的控制通道
        /// </summary>
        public ObservableCollection<ControlChannelSettingItemViewModel> ChannelSettings { get; } = []; 
        #endregion

        #region Methods
        protected override async Task ExecuteLoad()
        { 
            // 获取当前站点信息
            var stationSite = await _siteAggregateRepository.GetCurrentStationSiteAggregateAsync();
            if (stationSite is null)
            {
                throw new InvalidOperationException("Current station site aggregate is null.");
            }
            ChannelSettings.Clear();
            
            // 加载所有控制通道
            foreach (var channelInfo in stationSite.ControlChannelSignalInfos)
            {
                var settingItem = new ControlChannelSettingItemViewModel
                {
                    Id = channelInfo.ControlChannelInfo.ControllerId,
                    Name = channelInfo.ControlChannelInfo.ChannelName
                }; 
                // 如果有已保存的参数，则加载
                var savedParam = Parameter?.FirstOrDefault(p => p.ControlChannelId == channelInfo.ControlChannelInfo.ControllerId);
                if (savedParam != null)
                {
                    settingItem.Selected = true;
                    settingItem.SetSelectedControlModes(savedParam.AllowedControlModes);
                }
                ChannelSettings.Add(settingItem);
            } 
        }

        protected override string GetParameterJson()
        {
            var parameters = ChannelSettings
                .Where(s => s.Selected)
                .Select(s => new ControlOperationParamModel
                { 
                    ControlChannelId = s.Id,
                    ControlChannelName = s.Name,
                    AllowedControlModes = s.GetSelectedControlModes()
                })
                .ToList();

            return JsonConvert.SerializeObject(parameters);
        }

        #endregion
    }
}
