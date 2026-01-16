using System.Collections.ObjectModel;

using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters;

using Prism.Mvvm;

namespace MCCS.Interface.Components.ViewModels.Parameters
{
    /// <summary>
    /// 单个控制通道设置项的 ViewModel
    /// </summary>
    public class ControlChannelSettingItemViewModel : BindableBase
    {
        private int _channelIndex;
        /// <summary>
        /// 通道索引 (1-4)
        /// </summary>
        public int ChannelIndex
        {
            get => _channelIndex;
            set => SetProperty(ref _channelIndex, value);
        }

        private ControlChannelItem? _selectedChannel;
        /// <summary>
        /// 当前选中的控制通道
        /// </summary>
        public ControlChannelItem? SelectedChannel
        {
            get => _selectedChannel;
            set => SetProperty(ref _selectedChannel, value);
        }

        /// <summary>
        /// 控制模式选项列表（支持多选）
        /// </summary>
        public ObservableCollection<ControlModeOptionItem> ControlModeOptions { get; } = [];

        public ControlChannelSettingItemViewModel()
        {
            InitializeControlModeOptions();
        }

        private void InitializeControlModeOptions()
        {
            ControlModeOptions.Add(new ControlModeOptionItem
            {
                Mode = ControlModeTypeEnum.Manual,
                DisplayName = "手动控制",
                IsSelected = false
            });
            ControlModeOptions.Add(new ControlModeOptionItem
            {
                Mode = ControlModeTypeEnum.Static,
                DisplayName = "静态控制",
                IsSelected = false
            });
            ControlModeOptions.Add(new ControlModeOptionItem
            {
                Mode = ControlModeTypeEnum.Fatigue,
                DisplayName = "疲劳控制",
                IsSelected = false
            });
            ControlModeOptions.Add(new ControlModeOptionItem
            {
                Mode = ControlModeTypeEnum.Programmable,
                DisplayName = "程序控制",
                IsSelected = false
            });
        }

        /// <summary>
        /// 获取选中的控制模式列表
        /// </summary>
        public List<ControlModeTypeEnum> GetSelectedControlModes()
        {
            return ControlModeOptions
                .Where(o => o.IsSelected)
                .Select(o => o.Mode)
                .ToList();
        }

        /// <summary>
        /// 设置选中的控制模式
        /// </summary>
        public void SetSelectedControlModes(List<ControlModeTypeEnum> modes)
        {
            foreach (var option in ControlModeOptions)
            {
                option.IsSelected = modes.Contains(option.Mode);
            }
        }
    }
}
