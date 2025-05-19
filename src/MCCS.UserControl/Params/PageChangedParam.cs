namespace MCCS.UserControl.Params
{
    public record PageChangedParam
    {
        public PageChangedParam(int pageSize, int currentPage)
        {
            PageSize = pageSize;
            CurrentPage = currentPage;
        }

        public int PageSize { get; private set; } = 10; 
        public int CurrentPage { get; private set; } = 1;
    }
}
