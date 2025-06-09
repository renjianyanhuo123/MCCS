namespace MCCS.Models;

public class SensorData
{
    public string ActuatorId { get; set; }
    public double Force { get; set; }
    public double Displacement { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsValid { get; set; }
}
