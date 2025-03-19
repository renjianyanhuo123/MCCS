
using System.Windows.Input;

namespace MCCS.ViewModels.Pages
{
    public class HomeTestOperationPageViewModel : BaseViewModel
    {
        public const string Tag = "HomeTestOperationPage";

        private bool _action = false;

        public HomeTestOperationPageViewModel(IEventAggregator eventAggregator, IDialogService dialogService) 
            : base(eventAggregator, dialogService)
        {
        }

        #region 命令
        public DelegateCommand<object> MouseRightButtonDownCommand => new(ExcuateMouseRightButtonDownCommand);
        public DelegateCommand<object> MouseMoveCommand => new(ExcuateMouseMoveCommand);
        public DelegateCommand<object> MouseRightButtonUpCommand => new(ExcuateMouseRightButtonUpCommand);
        public DelegateCommand<object> MouseWheelCommand => new(ExcuateMouseWheelCommand);
        #endregion

        #region 私有方法
        private void ExcuateMouseRightButtonDownCommand(object param) 
        {

        }

        private void ExcuateMouseMoveCommand(object param) 
        {

        }

        private void ExcuateMouseRightButtonUpCommand(object param) 
        {

        }

        private void ExcuateMouseWheelCommand(object param) 
        {
            // 监听Crtl + 滚轮事件
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) 
            {

            }
        }
        #endregion
    }
}
