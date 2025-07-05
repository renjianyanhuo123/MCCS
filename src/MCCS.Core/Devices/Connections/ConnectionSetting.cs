namespace MCCS.Core.Devices.Connections
{
    public class ConnectionSetting
    {
        public string ConnectionId { get; set; } = string.Empty;

        public string ConnectionStr { get; set; } = string.Empty;

        public ConnectionTypeEnum ConnectionType { get; set; }
    }
}
