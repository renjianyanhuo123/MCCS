namespace MCCS.Services.AppExitService
{
    public interface IAppExitService
    {
        void Exit();

        Task ExitAsync();
    }
}
