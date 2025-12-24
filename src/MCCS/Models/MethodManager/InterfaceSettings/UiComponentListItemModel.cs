namespace MCCS.Models.MethodManager.InterfaceSettings
{
    public sealed class UiComponentListItemModel : BindableBase
    {
        public long NodeId { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _iconStr;
        public string IconStr
        {
            get => _iconStr;
            set => SetProperty(ref _iconStr, value);
        }
    }
}
