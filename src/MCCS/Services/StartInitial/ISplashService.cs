namespace MCCS.Services.StartInitial
{
    public interface ISplashService
    {
        Task InitializeConfigurationAsync();
        Task InitializeDatabaseAsync();
        Task RegisterServicesAsync();
        Task LoadModulesAsync();
        Task FinalizeAsync();
    }
}
