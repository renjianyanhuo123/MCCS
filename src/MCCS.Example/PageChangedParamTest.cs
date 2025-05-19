using MCCS.UserControl;

namespace MCCS.Example
{
    public class PageChangedParamTest:BindingBase
    {
        private int _pageSize;
        private int _currentPage;
        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }
    }
}
