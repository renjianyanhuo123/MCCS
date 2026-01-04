namespace MCCS.Station.Client
{
    public sealed class StationModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        public void OnInitialized(IContainerProvider containerProvider) => throw new NotImplementedException();
    }
}
