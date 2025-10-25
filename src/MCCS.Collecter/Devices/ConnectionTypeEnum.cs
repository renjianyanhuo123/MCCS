namespace MCCS.Collecter.Devices;

public enum ConnectionTypeEnum
{
    /// <summary>
    /// 串口通信
    /// </summary>
    SerialPort,
    /// <summary>
    /// TCP/IP通信
    /// </summary>
    TcpIp,
    Modbus,
    OPC,
    Mock
}