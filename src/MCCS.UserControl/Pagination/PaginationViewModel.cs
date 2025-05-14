using System.Windows;
using System.Windows.Input;

namespace MCCS.UserControl.Pagination
{
    public class PaginationViewModel : BindingBase
    {
        #region Fields
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _targetPage = 1;
        #endregion

        public PaginationViewModel()
        {

        }

        #region Properties
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
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
        public ICommand PreviousPageCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand JumpToPageCommand { get; private set; }
        #endregion
    }
}
