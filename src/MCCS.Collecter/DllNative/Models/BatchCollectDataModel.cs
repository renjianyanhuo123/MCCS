namespace MCCS.Collecter.DllNative.Models
{
    public record BatchCollectDataModel(
        long Timespan,
        int DeviceNumber,
        List<TNet_ADHInfo> DataPacks)
    {
        public long Timespan { get; private set; } = Timespan;

        public int DeviceNumber { get; private set; } = DeviceNumber;

        /// <summary>
        /// 数据包集合
        /// </summary>
        public List<TNet_ADHInfo> DataPacks { get; private set; } = DataPacks;
    }
}
