using MaterialDesignThemes.Wpf;

using System.Collections.ObjectModel;

using MCCS.Common.Resources.ViewModels;
using MCCS.Interface.Components.Models;
using MCCS.Models.CurveModels;
using MCCS.Station.Core.PseudoChannelManagers; 

namespace MCCS.ViewModels.Dialogs
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SetCurveDialogViewModel : BaseDialog
    {
        public const string Tag = "SetCurveDialog";

        private readonly IPseudoChannelManager _pseudoChannelManager;

        #region Property
        // 选中的数字值
        private int _selectedCount = 1;
        public int SelectedCount
        {
            get => _selectedCount;
            set => SetProperty(ref _selectedCount, value);
        }
        // 数字选项集合
        public ObservableCollection<int> CountOptions { get; private set; }
        public ObservableCollection<CurveMainModel> CurveModels { get; private set; } = [];
         
        /// <summary>
        /// X轴绑定的集合
        /// </summary>
        public ObservableCollection<XyBindCollectionItem> XBindCollection { get; } = [];
        /// <summary>
        /// Y轴绑定的集合
        /// </summary>
        public ObservableCollection<XyBindCollectionItem> YBindCollection { get; } = [];

        #endregion

        #region Command
        public DelegateCommand OkCommand => new(ExecuteOkCommand);
        public DelegateCommand CancelCommand => new(ExecuteCancelCommand);
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand CurveSelectionChangedCommand { get; } 

        #endregion
        
        public SetCurveDialogViewModel(IPseudoChannelManager pseudoManager)
        {
            //_containerProvider = containerProvider;
            // 初始化数字选项
            _pseudoChannelManager = pseudoManager;
            LoadCommand = new DelegateCommand(ExecuteLoadCommand);
            CurveSelectionChangedCommand = new DelegateCommand(ExecuteCurveSelectionChangedCommand); 
            CountOptions = [1, 2, 3, 4, 5, 6, 8, 9, 12];
        } 
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("title"))
            {
                Title = parameters.GetValue<string>("title");
            } 
        }
        #region private methods
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
            ExecuteCurveSelectionChangedCommand();
        } 
        /// <summary>
        /// 选择曲线触发
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void ExecuteCurveSelectionChangedCommand()
        {
            foreach (var item in CurveModels) item.Curve?.Dispose();
            CurveModels.Clear();
            for (var i = 0; i < SelectedCount; i++)
            {
                var xAxisInfo = XBindCollection.FirstOrDefault(s => s.Id == 0);
                var yAxisInfo = YBindCollection.FirstOrDefault();
                if (xAxisInfo != null && yAxisInfo != null)
                {
                    var forceModel = new CurveMainModel(xAxisInfo, yAxisInfo, _pseudoChannelManager);
                    CurveModels.Add(forceModel);
                }
            }
        }
        private void ExecuteOkCommand()
        {
            
        }

        private void ExecuteCancelCommand()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        #endregion
    }
}
