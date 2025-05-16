using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MCCS.UserControl.Pagination
{
    public class PaginationViewModel : BindingBase
    {
        #region Fields
        private int _currentPage = 1;
        private int _total = 1;
        private int _targetPage = 1;
        private ObservableCollection<SelectorUnitViewModel> _selectorUnits;
        #endregion

        public PaginationViewModel()
        {
            _selectorUnits = 
                [
                    new SelectorUnitViewModel
                    {
                        IsSelected = true,
                        Num = 1,
                        Type = UnitTypeEnum.TextBlock
                    },
                    new SelectorUnitViewModel
                    {
                        IsSelected = false,
                        Num = 2,
                        Type = UnitTypeEnum.TextBlock
                    },
                    new SelectorUnitViewModel
                    {
                        IsSelected = false,
                        Num = 3,
                        Type = UnitTypeEnum.TextBlock
                    }
                ];
        }

        #region Properties
        public ObservableCollection<SelectorUnitViewModel> SelectorUnits
        {
            get => _selectorUnits;
            set => SetProperty(ref _selectorUnits, value);
        }
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int Total
        {
            get => _total;
            set
            {
                if (_total == value) return;
                UpdateSelectorUnits(value);
                SetProperty(ref _total, value);
            }
        }

        public int TargetPage
        {
            get => _targetPage;
            set => SetProperty(ref _targetPage, value);
        }
        #endregion

        #region Commands

        public ICommand InitialCommand { get; private set; } = new RelayCommand( param => { });
        public ICommand PreviousPageCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand JumpToPageCommand { get; private set; }
        #endregion

        #region private method
        private void UpdateSelectorUnits(int total)
        {
            if (total <= 5)
            {
                for (var i = 1; i <= total; i++)
                {
                    _selectorUnits.Add(new SelectorUnitViewModel
                    {
                        IsSelected = true,
                        Num = i,
                        Type = UnitTypeEnum.TextBlock
                    });
                }
            }
            //else if ()
            //{

            //}
        }

        #endregion
    }
}
