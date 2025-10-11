namespace MCCS.WorkflowSetting.EventParams
{
    public record DeleteNodeEvent
    {
        /// <summary>
        /// 保存发布者引用
        /// </summary>
        public required string Source { get; set; }
        public required string NodeId { get; set; }
    }
}
