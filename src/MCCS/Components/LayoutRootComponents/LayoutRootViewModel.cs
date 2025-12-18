using System.Collections.ObjectModel;
using System.Windows.Controls;

using MCCS.ViewModels;

namespace MCCS.Components.LayoutRootComponents
{
    public class LayoutRootViewModel :BaseViewModel
    {
        public LayoutRootViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {

        }


        #region Property
        public ObservableCollection<GridSizeDefinitionModel> Rows { get; } = [];
        public ObservableCollection<GridSizeDefinitionModel> Columns { get; } = [];
        public ObservableCollection<CellViewModel> Cells { get; } = [];
        #endregion
    }
}
