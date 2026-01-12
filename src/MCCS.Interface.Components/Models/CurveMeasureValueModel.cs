namespace MCCS.Interface.Components.Models
{
    public record CurveMeasureValueModel
    {
        // 可以是时间戳、秒、毫秒等
        public double XValue { get; init; }  
        public double YValue { get; init; }
    }
}
