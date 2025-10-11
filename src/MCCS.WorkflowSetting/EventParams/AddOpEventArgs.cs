namespace MCCS.WorkflowSetting.EventParams
{
    public class AddOpEventArgs
    {
        /// <summary>
        /// 保存发布者引用
        /// </summary>
        public required object Source { get; init; }  
        public required string NodeId { get; set; }
    }
}
