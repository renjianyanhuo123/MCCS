namespace MCCS.Interface.Components.Models
{
    /// <summary>
    /// 组别
    /// </summary>
    public class InterfaceGroupModel : BindableBase
    {
        /// <summary>
        /// 组别名称
        /// </summary>
        private string _groupName = string.Empty;
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        /// <summary>
        /// 子项列表
        /// </summary>
        private List<UiComponentListItemModel> _items = [];
        public List<UiComponentListItemModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }
    }

    public sealed class UiComponentListItemModel : BindableBase
    {
        public string NodeId { get; set; }

        private string? _title;
        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string? _iconStr;
        public string? IconStr
        {
            get => _iconStr;
            set => SetProperty(ref _iconStr, value);
        }
    }
}
