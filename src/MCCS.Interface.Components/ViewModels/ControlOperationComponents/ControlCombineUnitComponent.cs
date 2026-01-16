using System.Collections.ObjectModel;

namespace MCCS.Interface.Components.ViewModels.ControlOperationComponents
{
    public sealed class ControlCombineUnitComponent : ControlUnitComponent
    {
        public event Action<ControlCombineUnitComponent, List<ControlCombineUnitChildComponent>> UnLockEvent;

        public ControlCombineUnitComponent(List<ControlCombineUnitChildComponent> children)
        {
            Title = string.Join('+', children.Select(s => s.ChannelName));
            Width = children.Count * 300;
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
            UnLockEvent?.Invoke(this, Children.ToList());
        }

        

        #endregion
    }
}
