namespace MCCS.Models.ControlCommand
{
    public class StaticControlModel
    {
        public ControlUnitTypeEnum UnitType { get; set; }

        public double Speed { get; set; }

        public double TargetValue { get; set; }
    }
}
