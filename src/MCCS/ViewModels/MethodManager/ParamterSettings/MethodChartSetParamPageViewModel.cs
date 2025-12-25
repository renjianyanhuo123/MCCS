using System.Collections.ObjectModel;

using MCCS.Collecter.PseudoChannelManagers;
using MCCS.Events.Mehtod.DynamicGridOperationEvents;
using MCCS.Models.CurveModels;
using MCCS.Models.MethodManager.ParamterSettings;

using Newtonsoft.Json;

namespace MCCS.ViewModels.MethodManager.ParamterSettings
{
    public sealed class MethodChartSetParamPageViewModel : BaseViewModel
    {
        private readonly IPseudoChannelManager _pseudoChannelManager;
        private ChartSettingParamModel? _parameter;
        private string _sourceId = string.Empty;

        public MethodChartSetParamPageViewModel(
            IPseudoChannelManager pseudoChannelManager, 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _pseudoChannelManager = pseudoChannelManager;
            LoadCommand = new DelegateCommand(ExecuteLoadCommand);
            SaveCommand = new DelegateCommand(ExecuteSaveCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameter = navigationContext.Parameters.GetValue<OpenParamterSetEventParam>("OpenParameterSetEventParam");
            _sourceId = parameter.SourceId;
            if (parameter is { Parameter: not null })
            { 
                _parameter = JsonConvert.DeserializeObject<ChartSettingParamModel>(parameter.Parameter); 
            }
        }

        private int _chartType;
        public int ChartType
        {
            get => _chartType;
            set => SetProperty(ref _chartType, value);
        }

        #region Command
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand SaveCommand { get; } 
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
        private void ExecuteLoadCommand()
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
            if (_parameter == null) return;
            SelectedXElement = XBindCollection.FirstOrDefault(c => c.Id == _parameter.XAxisParamId);
            SelectedYElement = YBindCollection.FirstOrDefault(c => c.Id == _parameter.YAxisParamId);
            ChartType = (int)_parameter.Type;
        }

        private void ExecuteSaveCommand()
        {
            var parameter = new ChartSettingParamModel
            {
                Type = (ChartTypeEnum)ChartType,
                XAxisParamId = SelectedXElement?.Id ?? 1,
                YAxisParamId = SelectedYElement?.Id ?? 0
            };
            if (_sourceId == string.Empty) return;
            var json = JsonConvert.SerializeObject(parameter);
            _eventAggregator.GetEvent<SaveParameterEvent>().Publish(new SaveParameterEventParam
            {
                SourceId = _sourceId,
                Parameter = json
            });
        }
        #endregion
    }
}
