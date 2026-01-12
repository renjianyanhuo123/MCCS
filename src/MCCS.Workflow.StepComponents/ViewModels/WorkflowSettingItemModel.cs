using System.Windows.Media;

using MCCS.Workflow.Contact.Models;

namespace MCCS.Workflow.StepComponents.ViewModels
{ 
    /// <summary>
    /// 组别
    /// </summary>
    public class WorkflowSettingGroupModel : BindableBase
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
        private List<WorkflowSettingItemModel> _items = [];
        public List<WorkflowSettingItemModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }
    }

    public class WorkflowSettingItemModel : BindableBase
    {
        public string Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name = string.Empty; 
        public string Name
        {
            get => _name; 
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 展示节点的类型
        /// </summary>
        private NodeDisplayTypeEnum _displayType;
        public NodeDisplayTypeEnum DisplayType
        {
            get => _displayType;
            set => SetProperty(ref _displayType, value);
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description; 
            set => SetProperty(ref _description, value);
        }

        private string _iconStr = string.Empty;
        public string IconStr
        {
            get => _iconStr;
            set => SetProperty(ref _iconStr, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private Brush _iconBackground = Brushes.Transparent; 
        public Brush IconBackground
        {
            get => _iconBackground;
            set => SetProperty(ref _iconBackground, value);
        } 
    }
}
