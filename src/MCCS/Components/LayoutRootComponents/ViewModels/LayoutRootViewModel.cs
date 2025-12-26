using MCCS.Events.Mehtod.DynamicGridOperationEvents;

namespace MCCS.Components.LayoutRootComponents.ViewModels
{
    public class LayoutRootViewModel : BindableBase
    {  

        /// <summary>
        /// 默认构造函数(当没有任何配置信息时)
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="eventAggregator"></param>
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
