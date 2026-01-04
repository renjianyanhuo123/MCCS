using System.Collections.ObjectModel;

using MCCS.Models.CurveModels;
using MCCS.Models.MethodManager.ParamterSettings;
using MCCS.Station.PseudoChannelManagers;

using Newtonsoft.Json;

namespace MCCS.ViewModels.MethodManager.ParamterSettings
{
    public sealed class MethodChartSetParamPageViewModel : BaseParameterSetViewModel<ChartSettingParamModel>
    {
        private readonly IPseudoChannelManager _pseudoChannelManager; 

        public MethodChartSetParamPageViewModel(
            IPseudoChannelManager pseudoChannelManager, 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _pseudoChannelManager = pseudoChannelManager;  
        }
         

        private int _chartType;
        public int ChartType
        {
            get => _chartType;
            set => SetProperty(ref _chartType, value);
        }

        #region Command 
        #endregion

        #region Property  
        /// <summary>
        /// X轴绑定的集合
        /// </summary>
        public ObservableCollection<XyBindCollectionItem> XBindCollection { get; } = [];
        /// <summary>
        /// Y轴绑定的集合
        /// </summary>
        public ObservableCollection<XyBindCollectionItem> YBindCollection { get; } = [];

        private XyBindCollectionItem? _selectedXElement; 
        public XyBindCollectionItem? SelectedXElement
        {
            get => _selectedXElement; 
            set => SetProperty(ref _selectedXElement, value);
        }

        private XyBindCollectionItem? _selectedYElement; 
        public XyBindCollectionItem? SelectedYElement
        {
            get => _selectedYElement; 
            set => SetProperty(ref _selectedYElement, value);
        }
        #endregion

        #region Private Method
        protected override void ExecuteLoad()
        {
            XBindCollection.Clear();
            YBindCollection.Clear();
            var channels = _pseudoChannelManager.GetPseudoChannels();
            foreach (var channel in channels)
            {
                var tempModel = new XyBindCollectionItem
                {
                    Id = channel.ChannelId,
                    Name = channel.Configuration.ChannelName,
                    Unit = channel.Configuration.Unit ?? "",
                    DisplayName = channel.Configuration.ChannelName
                };
                XBindCollection.Add(tempModel);
                YBindCollection.Add(tempModel);
            }
            XBindCollection.Add(new XyBindCollectionItem
            {
                Id = 0,
                Name = "Time",
                Unit = "s",
                DisplayName = "时间"
            });
            if (Parameter == null) return;
            SelectedXElement = XBindCollection.FirstOrDefault(c => c.Id == Parameter.XAxisParam?.Id);
            SelectedYElement = YBindCollection.FirstOrDefault(c => c.Id == Parameter.YAxisParam?.Id);
            ChartType = (int)Parameter.Type;
        } 
        #endregion

        protected override string GetParameterJson()
        {
            var parameter = new ChartSettingParamModel
            {
                Type = (ChartTypeEnum)ChartType,
                XAxisParam = SelectedXElement,
                YAxisParam = SelectedYElement
            };
            var json = JsonConvert.SerializeObject(parameter);
            return json;
        }
    }
}
