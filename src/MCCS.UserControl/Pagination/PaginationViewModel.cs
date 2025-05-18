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
        private int _pageSize = 10;
        private int _totalPages = 1;
        private ObservableCollection<SelectorUnitViewModel> _selectorUnits;
        #endregion

        public PaginationViewModel()
        {
            _selectorUnits = [];
        }

        public void UpdateFields(int total, int pageSize, int currentPage)
        {
            _total = total;
            _pageSize = pageSize;
            _currentPage = currentPage;
            UpdateSelectorUnits();
        }

        #region Properties
        public ObservableCollection<SelectorUnitViewModel> SelectorUnits
        {
            get => _selectorUnits;
            set => SetProperty(ref _selectorUnits, value);
        }
        public int CurrentPage
        {
            get;
            set;
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize == value) return;
                SetProperty(ref _pageSize, value);
                // UpdateSelectorUnits();
            }
        }

        public int Total
        {
            get => _total;
            set
            {
                if (_total == value) return;
                SetProperty(ref _total, value);
                // UpdateSelectorUnits();
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public int TargetPage
        {
            get => _targetPage;
            set => SetProperty(ref _targetPage, value);
        }
        #endregion

        #region Commands

        public ICommand InitialCommand { get; private set; } = new RelayCommand( param => { });
        public ICommand PreviousPageCommand => new RelayCommand(PreviousPage, _ => true);
        public ICommand NextPageCommand => new RelayCommand(NextPage, _ => true);
        public ICommand JumpToPageCommand => new RelayCommand(JumpToPage, _ => true);

        public ICommand SelectedChangedCommand => new RelayCommand(SelectedChanged, 
            param => param is string { Length: >= 2 });

        #endregion

        #region private method
        private void JumpToPage(object? param)
        {
            if (param is not int p || p > TotalPages || p < 1) return;
            CurrentPage = p;
            UpdateSelectorUnits();
        }

        private void PreviousPage(object? param)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdateSelectorUnits();
            }
        }
        private void NextPage(object? param)
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdateSelectorUnits();
            }
        }
        private void SelectedChanged(object? param)
        {
            if (param is not string s) return;
            var t = s.Remove(2, s.Length - 2);
            if (!int.TryParse(t, out var pageSize)) return;
            PageSize = pageSize;
            UpdateSelectorUnits();
        }

        private void UpdateSelectorUnits()
        {
            TotalPages = _total / _pageSize + 1;
            if (TotalPages < CurrentPage)
            {
                CurrentPage = 1;
            }
            _selectorUnits.Clear();
            for (var i = 1; i <= TotalPages; i++)
            {
                // 以当前页为中心，前后各显示2页;总共为5页
                if (i <= CurrentPage)
                {
                    if (CurrentPage <= 4)
                    {
                        for (var j = i; j <= CurrentPage; j++)
                        {
                            _selectorUnits.Add(new SelectorUnitViewModel
                            {
                                IsSelected = j == CurrentPage,
                                Num = j,
                                Type = UnitTypeEnum.TextBlock
                            });
                        }
                    }
                    else
                    {
                        _selectorUnits.Add(new SelectorUnitViewModel
                        {
                            IsSelected = i == CurrentPage,
                            Num = 1,
                            Type = UnitTypeEnum.TextBlock
                        });
                        _selectorUnits.Add(new SelectorUnitViewModel
                        {
                            IsSelected = false,
                            Type = UnitTypeEnum.Image
                        });
                        for (var j = CurrentPage - 2; j <= CurrentPage; j++)
                        {
                            _selectorUnits.Add(new SelectorUnitViewModel
                            {
                                IsSelected = j == CurrentPage,
                                Num = j,
                                Type = UnitTypeEnum.TextBlock
                            });
                        }
                    }
                    i = CurrentPage;
                }
                else
                {
                    if (_totalPages - CurrentPage > 3)
                    {
                        for (var j = i; j <= CurrentPage + 2; j++)
                        {
                            _selectorUnits.Add(new SelectorUnitViewModel
                            {
                                IsSelected = i == CurrentPage,
                                Num = j,
                                Type = UnitTypeEnum.TextBlock
                            });
                        }
                        _selectorUnits.Add(new SelectorUnitViewModel
                        {
                            IsSelected = false,
                            Num = i,
                            Type = UnitTypeEnum.Image
                        });
                        _selectorUnits.Add(new SelectorUnitViewModel
                        {
                            IsSelected = false,
                            Num = _totalPages,
                            Type = UnitTypeEnum.TextBlock
                        });
                    }
                    else
                    {
                        for (var j = i; j <= _totalPages; j++)
                        {
                            _selectorUnits.Add(new SelectorUnitViewModel
                            {
                                IsSelected = j == CurrentPage,
                                Num = j,
                                Type = UnitTypeEnum.TextBlock
                            });
                        }
                    }
                    i = _totalPages;
                }
            }
        }
        #endregion
    }
}
