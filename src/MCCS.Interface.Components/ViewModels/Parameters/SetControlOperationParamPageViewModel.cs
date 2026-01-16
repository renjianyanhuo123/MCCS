using MCCS.Infrastructure.Repositories;
using MCCS.Interface.Components.Models.ParamterModels.ControlOperationParameters;

namespace MCCS.Interface.Components.ViewModels.Parameters
{
    public sealed class SetControlOperationParamPageViewModel : BaseParameterSetViewModel<ControlOperationParamModel> 
    {
        private readonly IStationSiteAggregateRepository _siteAggregateRepository;

        public SetControlOperationParamPageViewModel(
            IStationSiteAggregateRepository siteAggregateRepository, 
            IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _siteAggregateRepository = siteAggregateRepository;
        }

        protected override async Task ExecuteLoad()
        {
            var stationSite = await _siteAggregateRepository.GetCurrentStationSiteAggregateAsync();
            if (stationSite is null) throw new InvalidOperationException("Current station site aggregate is null."); 
        }

        protected override string GetParameterJson() => throw new NotImplementedException();
    }
}
