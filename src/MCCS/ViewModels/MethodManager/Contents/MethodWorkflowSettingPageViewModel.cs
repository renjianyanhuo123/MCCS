using System.Windows.Controls;

using MCCS.Common.Resources.ViewModels;
using MCCS.WorkflowSetting.EventParams;

namespace MCCS.ViewModels.MethodManager.Contents
{
    public sealed class MethodWorkflowSettingPageViewModel : BaseViewModel
    {
        public const string Tag = "MethodWorkflowSettingPage";

        private long _methodId = -1;
        private double _oldWidth;
        private double _oldHeight;

        public MethodWorkflowSettingPageViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        { 
            LoadCommand = new DelegateCommand<object>(ExecuteLoadCommand);
            _eventAggregator.GetEvent<NotificationWorkflowChangedEvent>().Subscribe(OnNotificationWorkflowChangedEvent);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _methodId = navigationContext.Parameters.GetValue<long>("MethodId");
        }

        #region Property
        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private int _perect = 100;
        public int Perect
        {
            get => _perect;
            set
            {
                if (SetProperty(ref _perect, value))
                {
                    Scale = _perect / 100.0;
                }
            }
        }

        private double _scale = 1.0;
        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        #endregion

        #region Command 
        public DelegateCommand<object> LoadCommand { get; }  
        #endregion

        #region Private Method
        private void OnNotificationWorkflowChangedEvent(NotificationWorkflowChangedEventParam param)
        { 
            Height = param.Height + 100;
            if (Height < _oldHeight)
            {
                Height = _oldHeight;
            } 
            Width = param.Width + 100;
            if (Width < _oldWidth)
            {
                Width = _oldWidth;
            }
        }

        private void ExecuteLoadCommand(object param)
        {
            if (_methodId == -1) throw new ArgumentNullException("No MethodId!");
            if (param is Grid element)
            {
                _oldWidth = element.ActualWidth;
                _oldHeight = element.ActualHeight;
                Width = element.ActualWidth;
                Height = element.ActualHeight; 
            }
        }
        #endregion
    }
}
