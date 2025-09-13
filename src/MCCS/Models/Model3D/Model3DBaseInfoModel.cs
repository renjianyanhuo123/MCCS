using System.ComponentModel;

namespace MCCS.Models.Model3D
{
    public class Model3DBaseInfoModel
    {

        public long Id { get; set; }
        [DisplayName("模型名称")]
        public string Name { get; set; } = string.Empty;
        [DisplayName("是否正在使用")]
        public bool IsUse { get; set; }
        [DisplayName("创建时间")]
        public DateTimeOffset CreateTime { get; set; }
        [DisplayName("更新时间")]
        public DateTimeOffset UpdateTime { get; set; }
    }
}
