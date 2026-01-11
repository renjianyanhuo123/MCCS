namespace MCCS.Workflow.StepComponents.Models
{
    /// <summary>
    /// 联锁状态
    /// </summary>
    public class InterlockStatus
    {
        /// <summary>
        /// 是否全部通过
        /// </summary>
        public bool AllPassed { get; set; }

        /// <summary>
        /// 检查项列表
        /// </summary>
        public List<InterlockCheckItem> Items { get; set; } = new();

        /// <summary>
        /// 失败原因列表
        /// </summary>
        public List<string> FailureReasons { get; set; } = new();
    }

    /// <summary>
    /// 联锁检查项
    /// </summary>
    public class InterlockCheckItem
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否通过
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// 当前值
        /// </summary>
        public string? CurrentValue { get; set; }

        /// <summary>
        /// 期望值
        /// </summary>
        public string? ExpectedValue { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// 校准/核查报告
    /// </summary>
    public class VerificationReport
    {
        /// <summary>
        /// 报告ID
        /// </summary>
        public string ReportId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 报告路径
        /// </summary>
        public string ReportPath { get; set; } = string.Empty;

        /// <summary>
        /// 是否通过
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// 校准日期
        /// </summary>
        public DateTime CalibrationDate { get; set; }

        /// <summary>
        /// 有效期至
        /// </summary>
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// 标准器信息
        /// </summary>
        public string StandardInfo { get; set; } = string.Empty;

        /// <summary>
        /// 操作员
        /// </summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// 误差结果
        /// </summary>
        public List<CalibrationPoint> CalibrationPoints { get; set; } = new();

        /// <summary>
        /// 等级结论
        /// </summary>
        public string GradeConclusion { get; set; } = string.Empty;
    }

    /// <summary>
    /// 校准点
    /// </summary>
    public class CalibrationPoint
    {
        /// <summary>
        /// 标准值
        /// </summary>
        public double StandardValue { get; set; }

        /// <summary>
        /// 指示值
        /// </summary>
        public double IndicatedValue { get; set; }

        /// <summary>
        /// 误差
        /// </summary>
        public double Error { get; set; }

        /// <summary>
        /// 误差百分比
        /// </summary>
        public double ErrorPercent { get; set; }
    }

    /// <summary>
    /// 零点偏移结果
    /// </summary>
    public class ZeroOffsetResult
    {
        /// <summary>
        /// 通道ID
        /// </summary>
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; set; } = string.Empty;

        /// <summary>
        /// 偏移值
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// 漂移值
        /// </summary>
        public double Drift { get; set; }

        /// <summary>
        /// 是否在阈值内
        /// </summary>
        public bool WithinThreshold { get; set; }
    }

    /// <summary>
    /// 预载结果
    /// </summary>
    public class PreloadResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 峰值力
        /// </summary>
        public double PeakForce { get; set; }

        /// <summary>
        /// 峰值位移
        /// </summary>
        public double PeakDisplacement { get; set; }

        /// <summary>
        /// 残余位移
        /// </summary>
        public double ResidualDisplacement { get; set; }

        /// <summary>
        /// 是否有滑移报警
        /// </summary>
        public bool SlipAlarm { get; set; }

        /// <summary>
        /// 预载曲线文件路径
        /// </summary>
        public string CurvePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 段执行结果
    /// </summary>
    public class SegmentExecutionResult
    {
        /// <summary>
        /// 段ID
        /// </summary>
        public string SegmentId { get; set; } = string.Empty;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// 最小值
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// 结束值
        /// </summary>
        public double EndValue { get; set; }

        /// <summary>
        /// 执行时长（秒）
        /// </summary>
        public double DurationSeconds { get; set; }

        /// <summary>
        /// 段内事件
        /// </summary>
        public List<string> Events { get; set; } = new();

        /// <summary>
        /// 是否触发越限
        /// </summary>
        public bool LimitTriggered { get; set; }

        /// <summary>
        /// 越限类型
        /// </summary>
        public string? LimitType { get; set; }
    }

    /// <summary>
    /// 循环段执行结果
    /// </summary>
    public class CyclicExecutionResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 完成的循环数
        /// </summary>
        public int CompletedCycles { get; set; }

        /// <summary>
        /// 循环摘要列表
        /// </summary>
        public List<CycleSummary> CycleSummaries { get; set; } = new();

        /// <summary>
        /// 是否因停止准则中断
        /// </summary>
        public bool StoppedByCriteria { get; set; }

        /// <summary>
        /// 停止原因
        /// </summary>
        public string? StopReason { get; set; }
    }

    /// <summary>
    /// 循环摘要
    /// </summary>
    public class CycleSummary
    {
        /// <summary>
        /// 循环编号
        /// </summary>
        public int CycleNumber { get; set; }

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

        /// <summary>
        /// 等效刚度估计
        /// </summary>
        public double EquivalentStiffness { get; set; }

        /// <summary>
        /// 耗能（可选）
        /// </summary>
        public double? Energy { get; set; }
    }

    /// <summary>
    /// 停止准则评估结果
    /// </summary>
    public class StopCriteriaResult
    {
        /// <summary>
        /// 是否应该停止
        /// </summary>
        public bool ShouldStop { get; set; }

        /// <summary>
        /// 停止原因
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 触发的准则类型
        /// </summary>
        public string? TriggeredCriteria { get; set; }

        /// <summary>
        /// 当前值
        /// </summary>
        public double? CurrentValue { get; set; }

        /// <summary>
        /// 阈值
        /// </summary>
        public double? ThresholdValue { get; set; }
    }

    /// <summary>
    /// 数据采集结果
    /// </summary>
    public class AcquisitionResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 数据文件列表
        /// </summary>
        public List<string> DataFiles { get; set; } = new();

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalRecords { get; set; }

        /// <summary>
        /// 丢样数
        /// </summary>
        public long DroppedSamples { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// 报告生成结果
    /// </summary>
    public class ReportResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 报告路径
        /// </summary>
        public string ReportPath { get; set; } = string.Empty;

        /// <summary>
        /// 摘要指标
        /// </summary>
        public Dictionary<string, object?> SummaryMetrics { get; set; } = new();
    }

    /// <summary>
    /// 用户确认结果
    /// </summary>
    public class UserConfirmationResult
    {
        /// <summary>
        /// 是否确认
        /// </summary>
        public bool Confirmed { get; set; }

        /// <summary>
        /// 操作员
        /// </summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// 确认时间
        /// </summary>
        public DateTime ConfirmTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; } = string.Empty;

        /// <summary>
        /// 附件路径列表（照片等）
        /// </summary>
        public List<string> Attachments { get; set; } = new();
    }
}
