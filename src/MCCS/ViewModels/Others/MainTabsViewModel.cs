using System.Windows;

namespace MCCS.ViewModels.Others
{
    public class MainTabsViewModel : BindableBase
    {
        private string _id;
        public string Id { get => _id; set { SetProperty(ref _id, value); } }

        private string _content;
        public string Content { get => _content; set { SetProperty(ref _content, value); } }

        private bool _isEnable;
        public bool IsEnable { get => _isEnable; set { SetProperty(ref _isEnable, value); } }
    }
}
