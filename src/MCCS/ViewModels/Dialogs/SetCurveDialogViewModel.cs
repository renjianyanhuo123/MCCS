using MaterialDesignThemes.Wpf;
using MCCS.Models.Model3D;
using System.Collections.ObjectModel;
using MCCS.Core.Devices.Details;
using MCCS.Models;

namespace MCCS.ViewModels.Dialogs
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SetCurveDialogViewModel : BaseDialog
    {
        public const string Tag = "SetCurveDialog";

        #region Property
        // 选中的数字值
        private int _selectedCount = 1;
        public int SelectedCount
        {
            get => _selectedCount;
            set => SetProperty(ref _selectedCount, value);
        }
        // 数字选项集合
        public ObservableCollection<int> CountOptions { get; set; }
        public ObservableCollection<CurveShowModel> CurveModels { get; set; } = [];
        #endregion

        #region Command
        public DelegateCommand OkCommand => new(ExecuteOkCommand);
        public DelegateCommand CancelCommand => new(ExecuteCancelCommand);
        public AsyncDelegateCommand CurveSelectionChangedCommand => new(ExecuteCurveSelectionChangedCommand);
        #endregion
        
        public SetCurveDialogViewModel()
        {
            //_containerProvider = containerProvider;
            // 初始化数字选项
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
        /// <summary>
        /// 选择曲线触发
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Task ExecuteCurveSelectionChangedCommand()
        {
            CurveModels.Clear();
            for (var i = 0; i < SelectedCount; i++)
            {
                if (i % 2 == 0)
                {
                    var displacementModel = new CurveShowModel("Time(s)", "kN")
                    {
                        Title = "Time-Force"
                    };
                    CurveModels.Add(displacementModel);
                }
                else
                {
                    var forceModel = new CurveShowModel("Time(s)", "mm")
                    {
                        Title = "Time-Displace"
                    };
                    CurveModels.Add(forceModel);
                }
            }

            return Task.CompletedTask;
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
