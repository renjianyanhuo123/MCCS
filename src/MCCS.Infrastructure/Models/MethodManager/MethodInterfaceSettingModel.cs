using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.MethodManager
{
    [Table(Name = "method_interfaceSetting")]
    public class MethodInterfaceSettingModel : BaseModel
    {
        /// <summary>
        /// 方法ID
        /// </summary>
        public long MethodId { get; set; }

        /// <summary>
        /// 界面配置根节点
        /// </summary>
        [Column(IsNullable = false, StringLength = -2)]
        public required string RootSetting { get; set; }
    }
}
