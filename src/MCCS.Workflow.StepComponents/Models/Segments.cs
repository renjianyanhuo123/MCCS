namespace MCCS.Workflow.StepComponents.Models
{
    /// <summary>
    /// 段基类 - 描述一个控制段
    /// </summary>
    public abstract class Segment
    {
        /// <summary>
        /// 段ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 段名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 控制模式
        /// </summary>
        public ControlMode ControlMode { get; set; } = ControlMode.Displacement;

        /// <summary>
        /// 段类型
        /// </summary>
        public abstract string SegmentType { get; }
    }

    /// <summary>
    /// 斜坡段 - 从当前值线性变化到目标值
    /// </summary>
    public class RampSegment : Segment
    {
        public override string SegmentType => "Ramp";

        /// <summary>
        /// 目标值
        /// </summary>
        public double Target { get; set; }

        /// <summary>
        /// 变化速率（单位/秒）
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// 到达容差
        /// </summary>
        public double Tolerance { get; set; } = 0.01;

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public double TimeoutSeconds { get; set; } = 300;
    }

    /// <summary>
    /// 保持段 - 保持在当前值/设定值
    /// </summary>
    public class HoldSegment : Segment
    {
        public override string SegmentType => "Hold";

        /// <summary>
        /// 保持时长（秒）
        /// </summary>
        public double DurationSeconds { get; set; }

        /// <summary>
        /// 稳定带宽（允许波动范围）
        /// </summary>
        public double StabilityBand { get; set; } = 0.02;

        /// <summary>
        /// 记录周期（秒），用于蠕变/松弛
        /// </summary>
        public double LogIntervalSeconds { get; set; } = 1.0;
    }

    /// <summary>
    /// 循环段 - 循环加载
    /// </summary>
    public class CyclicSegment : Segment
    {
        public override string SegmentType => "Cyclic";

        /// <summary>
        /// 波形类型
        /// </summary>
        public WaveformType Waveform { get; set; } = WaveformType.Sine;

        /// <summary>
        /// 均值
        /// </summary>
        public double Mean { get; set; }

        /// <summary>
        /// 幅值
        /// </summary>
        public double Amplitude { get; set; }

        /// <summary>
        /// 频率（Hz）
        /// </summary>
        public double Frequency { get; set; } = 1.0;

        /// <summary>
        /// 循环次数
        /// </summary>
        public int Cycles { get; set; }

        /// <summary>
        /// 采样窗口：每 N 个周期采集一段原始波形
        /// </summary>
        public int EveryNCyclesWindow { get; set; } = 100;
    }

    /// <summary>
    /// 自定义点列段
    /// </summary>
    public class CustomPointListSegment : Segment
    {
        public override string SegmentType => "CustomPoints";

        /// <summary>
        /// 设定点列表
        /// </summary>
        public List<SetPoint> Points { get; set; } = new();

        /// <summary>
        /// 插值模式
        /// </summary>
        public InterpolationMode InterpMode { get; set; } = InterpolationMode.Linear;
    }

    /// <summary>
    /// 设定点
    /// </summary>
    public class SetPoint
    {
        /// <summary>
        /// 时间（秒）
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// 插值模式
    /// </summary>
    public enum InterpolationMode
    {
        /// <summary>
        /// 线性插值
        /// </summary>
        Linear,

        /// <summary>
        /// 阶梯（保持）
        /// </summary>
        Step,

        /// <summary>
        /// 样条插值
        /// </summary>
        Spline
    }
}
