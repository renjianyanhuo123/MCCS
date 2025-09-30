using Newtonsoft.Json;

namespace MCCS.Common.DataManagers.Methods
{
    public sealed class MethodContentItemModel : BindableBase
    {
        /// <summary>
        /// 方法文件位移ID
        /// </summary>
        public long Id { get; private set; }

        [JsonConstructor]
        public MethodContentItemModel(long id)
        {
            Id = id;
        }
        /// <summary>
        /// 方法基础信息
        /// </summary>
        [JsonIgnore]
        public MethodBaseInfo? MethodBaseInfo { get; private set; }

        public void SetBaseInfo(MethodBaseInfo baseInfo)
        {
            MethodBaseInfo = baseInfo;
        }
    }
}
