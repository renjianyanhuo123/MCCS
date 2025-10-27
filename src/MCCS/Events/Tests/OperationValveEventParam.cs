namespace MCCS.Events.Tests
{
    public record OperationValveEventParam
    {
        /// <summary>
        /// 模型ID
        /// </summary>
        public required string ModelId { get; init; }

        public bool IsOpen { get; init; }
    }
}
