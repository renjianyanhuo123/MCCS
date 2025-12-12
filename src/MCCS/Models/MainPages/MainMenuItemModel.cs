using MaterialDesignThemes.Wpf;

using MCCS.Infrastructure.Models.SystemSetting;

namespace MCCS.Models.MainPages
{
    /*
     *  Icon = StringToIcon.ConvertToIcon(s.Icon),
                Label = s.Name,
                Tag = s.Key
     */
    public class MainMenuItemModel : BindableBase
    {
        public string? Key { get; set; }

        private string? _name;
        public string? Name { get => _name; set => SetProperty(ref _name, value); }
        public PackIconKind SelectedIcon { get; set; }
        public PackIconKind UnselectedIcon { get; set; }
        public MenuType Type { get; set; }
        public bool IsEnabled { get; set; } = true; 
    }
}
