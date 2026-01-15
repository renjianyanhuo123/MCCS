namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    public sealed class ControlSingleUnitComponent : ControlUnitComponent
    {
        /// <summary>
        /// 是否参与组合控制
        /// </summary>
        private bool _isParticipateCombineControl = false; 
        public bool IsParticipateCombineControl
        {
            get => _isParticipateCombineControl;
            set => SetProperty(ref _isParticipateCombineControl, value);
        }

        private ControlCombineUnitChildComponent? _childComponent; 
        public ControlCombineUnitChildComponent? ChildComponent
        {
            get => _childComponent;
            set => SetProperty(ref _childComponent, value);
        }
    }
}
