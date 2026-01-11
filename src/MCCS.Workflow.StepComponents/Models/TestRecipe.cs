namespace MCCS.Workflow.StepComponents.Models
{
    /// <summary>
    /// 试验配方 - 完整描述一次试验的参数和流程
    /// </summary>
    public class TestRecipe
    {
        /// <summary>
        /// 配方ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 配方名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 试验类型
        /// </summary>
        public TestType TestType { get; set; } = TestType.StaticMonotonic;

        /// <summary>
        /// 控制模式
        /// </summary>
        public ControlMode ControlMode { get; set; } = ControlMode.Displacement;

        /// <summary>
        /// 通道映射
        /// </summary>
        public ChannelMap Channels { get; set; } = new();

        /// <summary>
        /// 安全限值
        /// </summary>
        public SafetyLimits Limits { get; set; } = new();

        /// <summary>
        /// 采样计划
        /// </summary>
        public SamplingPlan Sampling { get; set; } = new();

        /// <summary>
        /// 控制程序段列表
        /// </summary>
        public List<Segment> Program { get; set; } = new();

        /// <summary>
        /// 停机准则
        /// </summary>
        public StopCriteria Stop { get; set; } = new();

        /// <summary>
        /// 校准策略
        /// </summary>
        public CalibrationPolicy Calibration { get; set; } = new();

        /// <summary>
        /// 预载配置
        /// </summary>
        public PreloadConfig Preload { get; set; } = new();

        /// <summary>
        /// 配方版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; } = string.Empty;
    }

    /// <summary>
    /// 通道映射
    /// </summary>
    public class ChannelMap
    {
        /// <summary>
        /// 力通道列表
        /// </summary>
        public List<ChannelInfo> ForceChannels { get; set; } = new();

        /// <summary>
        /// 位移通道列表
        /// </summary>
        public List<ChannelInfo> DisplacementChannels { get; set; } = new();

        /// <summary>
        /// 应变通道列表
        /// </summary>
        public List<ChannelInfo> StrainChannels { get; set; } = new();

        /// <summary>
        /// 引伸计通道列表
        /// </summary>
        public List<ChannelInfo> ExtensometerChannels { get; set; } = new();

        /// <summary>
        /// 其他通道列表
        /// </summary>
        public List<ChannelInfo> OtherChannels { get; set; } = new();
    }

    /// <summary>
    /// 通道信息
    /// </summary>
    public class ChannelInfo
    {
        /// <summary>
        /// 通道ID
        /// </summary>
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// 通道名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 通道类型
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 量程
        /// </summary>
        public double Range { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 安全限值
    /// </summary>
    public class SafetyLimits
    {
        /// <summary>
        /// 最大力（kN）
        /// </summary>
        public double MaxForce { get; set; } = double.MaxValue;

        /// <summary>
        /// 最小力（kN）
        /// </summary>
        public double MinForce { get; set; } = double.MinValue;

        /// <summary>
        /// 最大位移（mm）
        /// </summary>
        public double MaxDisplacement { get; set; } = double.MaxValue;

        /// <summary>
        /// 最小位移（mm）
        /// </summary>
        public double MinDisplacement { get; set; } = double.MinValue;

        /// <summary>
        /// 最大应变（με）
        /// </summary>
        public double MaxStrain { get; set; } = double.MaxValue;

        /// <summary>
        /// 最大速率
        /// </summary>
        public double MaxRate { get; set; } = double.MaxValue;

        /// <summary>
        /// 最小油压（MPa）
        /// </summary>
        public double MinOilPressure { get; set; } = 0;

        /// <summary>
        /// 最大油压（MPa）
        /// </summary>
        public double MaxOilPressure { get; set; } = 35;

        /// <summary>
        /// 是否启用急停检测
        /// </summary>
        public bool EnableEmergencyStop { get; set; } = true;

        /// <summary>
        /// 是否启用限位检测
        /// </summary>
        public bool EnableLimitSwitch { get; set; } = true;
    }

    /// <summary>
    /// 采样计划
    /// </summary>
    public class SamplingPlan
    {
        /// <summary>
        /// 采样频率（Hz）
        /// </summary>
        public double SampleRate { get; set; } = 1000;

        /// <summary>
        /// 抽稀因子
        /// </summary>
        public int DecimationFactor { get; set; } = 1;

        /// <summary>
        /// 触发模式
        /// </summary>
        public string TriggerMode { get; set; } = "Continuous";

        /// <summary>
        /// 数据格式
        /// </summary>
        public string DataFormat { get; set; } = "CSV";

        /// <summary>
        /// 是否分段存储
        /// </summary>
        public bool SegmentedStorage { get; set; } = false;

        /// <summary>
        /// 每段最大记录数
        /// </summary>
        public int MaxRecordsPerSegment { get; set; } = 100000;
    }

    /// <summary>
    /// 停机准则
    /// </summary>
    public class StopCriteria
    {
        /// <summary>
        /// 是否在破坏时停止
        /// </summary>
        public bool StopOnFailure { get; set; } = true;

        /// <summary>
        /// 破坏判定：力下降比例（如 0.2 表示下降 20%）
        /// </summary>
        public double FailureForceDropRatio { get; set; } = 0.2;

        /// <summary>
        /// 是否在达到最大循环数时停止
        /// </summary>
        public bool StopOnMaxCycles { get; set; } = true;

        /// <summary>
        /// 最大循环数
        /// </summary>
        public int MaxCycles { get; set; } = 1000000;

        /// <summary>
        /// 是否在达到最大时间时停止
        /// </summary>
        public bool StopOnMaxTime { get; set; } = false;

        /// <summary>
        /// 最大时间（秒）
        /// </summary>
        public double MaxTimeSeconds { get; set; } = 86400;

        /// <summary>
        /// 是否在刚度衰减时停止
        /// </summary>
        public bool StopOnStiffnessDegradation { get; set; } = false;

        /// <summary>
        /// 刚度衰减阈值比例
        /// </summary>
        public double StiffnessDegradationRatio { get; set; } = 0.5;

        /// <summary>
        /// 是否在传感器异常时停止
        /// </summary>
        public bool StopOnSensorAnomaly { get; set; } = true;
    }

    /// <summary>
    /// 校准策略
    /// </summary>
    public class CalibrationPolicy
    {
        /// <summary>
        /// 是否强制校准
        /// </summary>
        public bool RequireCalibration { get; set; } = false;

        /// <summary>
        /// 是否强制核查
        /// </summary>
        public bool RequireVerification { get; set; } = true;

        /// <summary>
        /// 校准有效期（天）
        /// </summary>
        public int CalibrationValidDays { get; set; } = 365;

        /// <summary>
        /// 核查有效期（天）
        /// </summary>
        public int VerificationValidDays { get; set; } = 30;
    }

    /// <summary>
    /// 预载配置
    /// </summary>
    public class PreloadConfig
    {
        /// <summary>
        /// 是否启用预载
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 预载目标值
        /// </summary>
        public double TargetValue { get; set; }

        /// <summary>
        /// 预载速率
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// 预载控制模式
        /// </summary>
        public ControlMode ControlMode { get; set; } = ControlMode.Force;

        /// <summary>
        /// 保持时间（秒）
        /// </summary>
        public double HoldSeconds { get; set; } = 10;

        /// <summary>
        /// 预载后是否回零
        /// </summary>
        public bool ReturnToZero { get; set; } = false;
    }
}
