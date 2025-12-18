using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using MCCS.UserControl;
using MCCS.UserControl.DynamicGrid;
using MCCS.UserControl.Transfer;

namespace MCCS.Example
{
    public class MainViewModel : BindingBase
    {
        // private object _param;
        //private int _total;
        public MainViewModel()
        {
            //Total = 317;
            LayoutSetting = new LayoutSettingModel
            {
                Rows =
                [
                    new GridSizeDefinitionModel
                    {
                        Value = 1,
                        UnitType = GridUnitType.Star
                    }
                ],
                Columns = [
                    new GridSizeDefinitionModel
                {
                    Value = 1,
                    UnitType = GridUnitType.Star
                }],
                Cells = [
                    new CellViewModel
                    {
                        Column = 0,
                        Row = 0,
                        Content = new CellItemControl()
                        {
                            Width = 400,
                            Height = 200
                        }
                    }
                ]
            };
        }

        //public int Total
        //{
        //    get => _total;
        //    set => SetProperty(ref _total, value);
        //}
        

        //public ICommand OnPageChanged => new RelayCommand(param =>
        //{
        //    var p = param as PageChangedParam;
        //    Thread.Sleep(1000);
        //    Debug.WriteLine($"当前页:{p?.CurrentPage},PageSize:{p?.PageSize}");
        //}, _ => true);

        //private string _sourceName = "123123123";

        //public string SourceName
        //{
        //    get => _sourceName;
        //    set => SetProperty(ref _sourceName, value);
        //}

        //private string _targetName = "2132131"; 
        //public string TargetName
        //{
        //    get => _targetName;
        //    set => SetProperty(ref _targetName, value);
        //}

        //public ObservableCollection<TransferItemModel> SourceList { get; } = [
        //    new()
        //    {
        //        Id = Guid.NewGuid().ToString("N"),
        //        Name = "12333",
        //        IsSelected = true
        //    },
        //    new()
        //    {
        //        Id = Guid.NewGuid().ToString("N"),
        //        Name = "HHHH",
        //        IsSelected = true
        //    }
        //];

        //public ObservableCollection<TransferItemModel> TargetList { get; } = [];

        private LayoutSettingModel _layoutSettingModel;
        public LayoutSettingModel LayoutSetting { get => _layoutSettingModel; set => SetProperty(ref _layoutSettingModel, value); }

    }
}
