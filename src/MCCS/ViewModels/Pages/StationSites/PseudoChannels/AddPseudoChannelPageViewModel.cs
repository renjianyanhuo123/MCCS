using System.Collections.ObjectModel;
using MCCS.Core.Repositories;
using MCCS.Models.Stations.PseudoChannels;

namespace MCCS.ViewModels.Pages.StationSites.PseudoChannels
{
    public sealed class AddPseudoChannelPageViewModel : BaseViewModel
    {
        public const string Tag = "AddPseudoChannelPage";

        private readonly IStationSiteAggregateRepository _stationSiteAggregateRepository;

        private long _stationId = -1;

        public AddPseudoChannelPageViewModel(IStationSiteAggregateRepository stationSiteAggregateRepository,
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _stationSiteAggregateRepository = stationSiteAggregateRepository;
        }

        #region Property
        private string _channelName = string.Empty;
        public string ChannelName
        {
            get => _channelName;
            set => SetProperty(ref _channelName, value);
        }

        private string _internalId = string.Empty;
        public string InternalId
        {
            get => _internalId;
            set => SetProperty(ref _internalId, value);
        }

        /// <summary>
        /// 范围最小值
        /// </summary>
        private double _rangeMin;
        public double RangeMin { get => _rangeMin; set => SetProperty(ref _rangeMin, value); }

        /// <summary>
        /// 范围最大值
        /// </summary>
        private double _rangeMax;
        public double RangeMax { get => _rangeMax; set => SetProperty(ref _rangeMax, value); }

        /// <summary>
        /// 计算公式
        /// </summary>
        private string _formula;
        public string Formula { get => _formula; set => SetProperty(ref _formula, value); }

        /// <summary>
        /// 是否可校准
        /// </summary>
        private bool _hasTare;
        public bool HasTare { get => _hasTare; set => SetProperty(ref _hasTare, value); }

        public ObservableCollection<PseudoChannelBindedSignalViewModel> BindedSignals { get; } = [];

        #endregion
    }
}
