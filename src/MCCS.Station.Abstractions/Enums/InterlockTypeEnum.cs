namespace MCCS.Station.Abstractions.Enums
{
    /// <summary>
    /// 联锁类型
    /// </summary>
    public enum InterlockTypeEnum
    {
        EStop,          // 急停
        DoorOpen,       // 门禁
        OilPressure,    // 油压
        OilTemperature, // 油温
        TravelLimit,    // 行程限位
        GripClosed      // 夹具闭合
    }
}
