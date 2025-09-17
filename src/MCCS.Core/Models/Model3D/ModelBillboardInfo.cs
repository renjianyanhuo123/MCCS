using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.Model3D
{
    [Table(Name = "tb_modelBillboardInfo")]
    public class ModelBillboardInfo : BaseModel
    {
        /// <summary>
        /// 绑定的3D模型文件ID
        /// </summary>
        public long ModelFileId { get; set; }
        /// <summary>
        /// 绑定的控制通道ID
        /// </summary>
        public long ControlChannelId { get; set; }
        /// <summary>
        /// 绑定的整体模型ID(方便加载)
        /// </summary>
        public long ModelId { get; set; }
        /// <summary>
        /// 看板背景颜色
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public string BackgroundColor { get; set; } = string.Empty;
        /// <summary>
        /// 看板名称
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public string BillboardName { get; set; } = string.Empty;
        /// <summary>
        /// 字体颜色
        /// </summary>
        [Column(IsNullable = false, StringLength = 50)]
        public string FontColor { get; set; } = string.Empty;
        /// <summary>
        /// 字体大小
        /// </summary>
        public double Scale { get; set; }

        [Column(IsNullable = false, StringLength = 50)]
        public string PositionStr { get; set; } = string.Empty;
    }
}
