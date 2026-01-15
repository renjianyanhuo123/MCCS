using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

using MCCS.Common.Resources.ViewModels;
using MCCS.Infrastructure.Helper;
using MCCS.Interface.Components.Enums;
using MCCS.Interface.Components.Events;
using MCCS.Interface.Components.Models;
using MCCS.Interface.Components.Registry;

namespace MCCS.Interface.Components.ViewModels
{
    /// <summary>
    /// 组件列表页面 ViewModel
    /// </summary>
    public class MethodComponentsPageViewModel : BaseViewModel
    { 
        private readonly IInterfaceRegistry _interfaceRegistry;
        private string _sourceId = "";

        public MethodComponentsPageViewModel(IEventAggregator eventAggregator, IInterfaceRegistry interfaceRegistry) : base(eventAggregator)
        {
            _interfaceRegistry = interfaceRegistry; 
            LoadCommand = new DelegateCommand(ExecuteLoadCommand);
            SelectComponentCommand = new DelegateCommand<UiComponentListItemModel>(ExecuteSelectComponentCommand);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var sourceId = navigationContext.Parameters.GetValue<string>("SourceId");
            _sourceId = sourceId;
        }

        #region Command
        public DelegateCommand LoadCommand { get; }

        public DelegateCommand<UiComponentListItemModel> SelectComponentCommand { get; }
        #endregion

        #region Private Method 
        private void ExecuteLoadCommand()
        {
            GroupModels.Clear();
            foreach (InterfaceComponentCategory status in Enum.GetValues(typeof(InterfaceComponentCategory)))
            {
                var description = EnumHelper.GetDescription(status);
                var components = _interfaceRegistry.GetComponentsByCategory(status);
                if (components.Count == 0) continue;
                var groupModel = new InterfaceGroupModel
                {
                    GroupName = description,
                    Items = components.Select(s => new UiComponentListItemModel
                    {
                        NodeId = s.Id,
                        Title = s.Name,
                        IconStr = s.Icon
                    }).ToList()
                };
                GroupModels.Add(groupModel);
            } 
        }

        private void ExecuteSelectComponentCommand(UiComponentListItemModel param)
        {
            if (_sourceId == string.Empty) return;
            _eventAggregator.GetEvent<SelectedComponentEvent>().Publish(new SelectedComponentEventParam
            {
                SourceId = _sourceId,
                NodeId = param.NodeId
            });
        }
        #endregion

        #region Property
        public ObservableCollection<InterfaceGroupModel> GroupModels { get; } = [];
        #endregion
    }
}
