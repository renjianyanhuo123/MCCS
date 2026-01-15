using System.Collections.ObjectModel;

namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    public sealed class ControlCombineUnitComponent : ControlUnitComponent
    {
        public ControlCombineUnitComponent(List<ControlCombineUnitChildComponent> children)
        {
            Children.AddRange(children);
            UnLockCommand = new DelegateCommand(ExecuteUnLockCommand); 
        }

        #region Property
        public ObservableCollection<ControlCombineUnitChildComponent> Children { get; } = [];
        #endregion

        #region Command 
        public DelegateCommand UnLockCommand { get; }
        

        #endregion

        #region Private Method
        private void ExecuteUnLockCommand()
        {

        }

        

        #endregion
    }
}
