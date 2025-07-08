using System.Windows;

namespace MCCS.ViewModels.Others
{
    public class MainTabsViewModel : BindableBase
    {
        private string _id = string.Empty;
        public string Id {
            get => _id; 
            set => SetProperty(ref _id, value);
        }

        private string _content = string.Empty;
        public string Content 
        { 
            get => _content; 
            set => SetProperty(ref _content, value);
        }

        private Visibility _isEnableClose;
        public Visibility IsEnableClose { get => _isEnableClose; set => SetProperty(ref _isEnableClose, value); }

        private bool _isChecked;
        public bool IsChecked 
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        private bool _isEnable;
        public bool IsEnable { get => _isEnable; set => SetProperty(ref _isEnable, value); }
    }
}
