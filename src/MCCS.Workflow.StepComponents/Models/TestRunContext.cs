namespace MCCS.Workflow.StepComponents.Models
{
    /// <summary>
    /// 试验运行上下文 - 一次试验的"唯一事实源"
    /// </summary>
    public class TestRunContext
    {
        /// <summary>
        /// 运行ID
        /// </summary>
        public Guid RunId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 试验配方
        /// </summary>
        public TestRecipe Recipe { get; set; } = new();

        /// <summary>
        /// 设备状态
        /// </summary>
        public MachineState Machine { get; set; } = new();

        /// <summary>
        /// 实时测量快照
        /// </summary>
        public MeasurementSnapshot Measurement { get; set; } = new();

        /// <summary>
        /// 数据文件目录
        /// </summary>
        public string DataFolder { get; set; } = string.Empty;

        /// <summary>
        /// 事件日志
        /// </summary>
        public List<EventLogEntry> Events { get; set; } = new();

        /// <summary>
        /// 步骤产物（校准证书号、对中结果等）
        /// </summary>
        public Dictionary<string, object?> StepOutputs { get; set; } = new();

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 当前段索引
        /// </summary>
        public int CurrentSegmentIndex { get; set; } = 0;

        /// <summary>
        /// 当前循环数
        /// </summary>
        public int CurrentCycleCount { get; set; } = 0;

        /// <summary>
        /// 操作者
        /// </summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// 试件编号
        /// </summary>
        public string SpecimenId { get; set; } = string.Empty;

        /// <summary>
        /// 添加事件日志
        /// </summary>
        public void AddEvent(string source, string message, EventLevel level = EventLevel.Info)
        {
            Events.Add(new EventLogEntry
            {
                Timestamp = DateTime.Now,
                Source = source,
                Message = message,
                Level = level
            });
        }

        /// <summary>
        /// 设置步骤输出
        /// </summary>
        public void SetStepOutput(string key, object? value)
        {
            StepOutputs[key] = value;
        }

        /// <summary>
        /// 获取步骤输出
        /// </summary>
        public T? GetStepOutput<T>(string key)
        {
            if (StepOutputs.TryGetValue(key, out var value) && value is T result)
            {
                return result;
            }
            return default;
        }
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    public class MachineState
    {
        /// <summary>
        /// 是否已使能
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 当前控制模式
        /// </summary>
        public ControlMode CurrentMode { get; set; } = ControlMode.Idle;

        /// <summary>
        /// 是否有报警
        /// </summary>
        public bool HasAlarm { get; set; }

        /// <summary>
        /// 报警信息列表
        /// </summary>
        public List<string> Alarms { get; set; } = new();

        /// <summary>
        /// 当前设定值
        /// </summary>
        public double CurrentSetpoint { get; set; }

        /// <summary>
        /// 液压站是否运行
        /// </summary>
        public bool HydraulicRunning { get; set; }

        /// <summary>
        /// 当前油压（MPa）
        /// </summary>
        public double OilPressure { get; set; }

        /// <summary>
        /// 设备连接状态
        /// </summary>
        public Dictionary<string, bool> DeviceConnections { get; set; } = new();

        /// <summary>
        /// 设备版本信息
        /// </summary>
        public Dictionary<string, string> DeviceVersions { get; set; } = new();
    }

    /// <summary>
    /// 实时测量快照
    /// </summary>
    public class MeasurementSnapshot
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// 力（kN）
        /// </summary>
        public double Force { get; set; }

        /// <summary>
        /// 位移（mm）
        /// </summary>
        public double Displacement { get; set; }

        /// <summary>
        /// 应变（με）
        /// </summary>
        public double Strain { get; set; }

        /// <summary>
        /// 试验时间（秒）
        /// </summary>
        public double ElapsedSeconds { get; set; }

        /// <summary>
        /// 当前循环数
        /// </summary>
        public int CycleCount { get; set; }

        /// <summary>
        /// 所有通道数据
        /// </summary>
        public Dictionary<string, double> ChannelValues { get; set; } = new();

        /// <summary>
        /// 峰值力
        /// </summary>
        public double PeakForce { get; set; }

        /// <summary>
        /// 谷值力
        /// </summary>
        public double ValleyForce { get; set; }

        /// <summary>
        /// 峰值位移
        /// </summary>
        public double PeakDisplacement { get; set; }

        /// <summary>
        /// 谷值位移
        /// </summary>
        public double ValleyDisplacement { get; set; }
    }

    /// <summary>
    /// 事件日志条目
    /// </summary>
    public class EventLogEntry
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 级别
        /// </summary>
        public EventLevel Level { get; set; } = EventLevel.Info;

        /// <summary>
        /// 操作者
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        public Dictionary<string, object?>? Data { get; set; }
    }

    /// <summary>
    /// 事件级别
    /// </summary>
    public enum EventLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
}
