namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    public class ControlUnitComponent : BindableBase
    {
        public ControlUnitComponent()
        {
            ApplyCommand = new DelegateCommand(ExecuteApplyCommand);
            StopCommand = new DelegateCommand(ExecuteStopCommand);
        }

        private string _controlUnitId = ""; 
        public string ControlUnitId
        {
            get => _controlUnitId;
            set => SetProperty(ref _controlUnitId, value);
        }

        private double _width = 0.0; 
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height = 0.0; 
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private string _title = string.Empty; 
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand StopCommand { get; }

        #region Private Method

        protected virtual void ExecuteApplyCommand()
        {
        }

        protected virtual void ExecuteStopCommand()
        { 
        }
        #endregion
    }
}
