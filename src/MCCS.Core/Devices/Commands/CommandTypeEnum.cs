using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Commands
{
    public enum CommandTypeEnum:int
    {
        // 通用指令
        Reset,
        Calibrate,
        SetParameter,
        GetParameter,
        StartMeasurement,
        StopMeasurement,

        // 特定设备指令
        SetMove,
        SetPressureUnit,
        SetSamplingRate,
        TriggerSelfTest,

        // 控制指令
        OpenValve,
        CloseValve,
        SetOutput,
        Emergency
    }
}
