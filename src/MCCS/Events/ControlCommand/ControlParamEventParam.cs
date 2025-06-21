using MCCS.Models;

namespace MCCS.Events.ControlCommand
{
    public class ControlParamEventParam
    {
        public ControlMode ControlMode { get; set; }

        public object Param { get; set; }
    }
}
