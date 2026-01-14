using MCCS.Common.Resources.ViewModels;
using MCCS.Interface.Components.Events;

using Newtonsoft.Json;

namespace MCCS.Interface.Components.ViewModels.Parameters
{
    /// <summary>
    /// 基类
    /// </summary>
    public abstract class BaseParameterSetViewModel<T> : BaseViewModel where T : class
    {
        protected string SourceId = string.Empty; 
        protected T? Parameter; 

        protected BaseParameterSetViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        { 
            SaveCommand = new DelegateCommand(ExecuteSaveCommand);
        }

        public DelegateCommand SaveCommand { get; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameter = navigationContext.Parameters.GetValue<OpenParamterSetEventParam>("OpenParameterSetEventParam");
            SourceId = parameter.SourceId;
            Parameter = null;
            if (parameter is { Parameter: not null })
            { 
                Parameter = JsonConvert.DeserializeObject<T>(parameter.Parameter);
            } 
            ExecuteLoad();
        }

        protected abstract Task ExecuteLoad();

        protected abstract string GetParameterJson();

        private void ExecuteSaveCommand()
        {
            var parameter = GetParameterJson();
            if (SourceId == string.Empty) return; 
            _eventAggregator.GetEvent<SaveParameterEvent>().Publish(new SaveParameterEventParam
            {
                SourceId = SourceId,
                Parameter = parameter
            });
        }
    }
}
