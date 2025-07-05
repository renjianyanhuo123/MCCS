namespace MCCS.Core.Devices.Connections
{
    public interface IConnectionManager : IDisposable
    {
        void RegisterConnection(ConnectionSetting connectionSetting);

        Task OpenAllConnections();

        IDeviceConnection? GetConnection(string connectionId);

        void RemoveConnection(string connectionId);

        void RegisterBatchConnections(List<ConnectionSetting> connectionSettings);
    }
}
