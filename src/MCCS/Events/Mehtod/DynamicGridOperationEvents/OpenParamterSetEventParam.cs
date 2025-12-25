namespace MCCS.Events.Mehtod.DynamicGridOperationEvents
{
    public record OpenParamterSetEventParam
    {
        /// <summary>
        /// 记录源节点
        /// </summary>
        public required string SourceId { get; init; }

        public string? Parameter { get; init; }
    }
}
