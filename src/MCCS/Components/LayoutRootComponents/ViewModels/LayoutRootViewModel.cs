using MCCS.Events.Mehtod.DynamicGridOperationEvents;

namespace MCCS.Components.LayoutRootComponents.ViewModels
{
    public class LayoutRootViewModel : BindableBase
    {

        public LayoutRootViewModel(LayoutNode rootNode, IEventAggregator eventAggregator)
        {
            _rootNode = rootNode; 
            eventAggregator.GetEvent<ChangedRootEvent>().Subscribe(param =>
            {
                RootNode = param.Root;
            });
        }
         

        private LayoutNode _rootNode;
        public LayoutNode RootNode { get => _rootNode; set => SetProperty(ref _rootNode, value); } 
    }
}
