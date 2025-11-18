using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.Model3D
{
    [Table(Name = "tb_model3d")]
    public class Model3DData : BaseModel
    {
        [Column(IsNullable = false, StringLength = 100)]
        public required string Key { get; set; }

        [Column(IsNullable = false, StringLength = 100)]
        public required string Name { get; set; }
        /// <summary>
        /// 基础信息ID
        /// </summary>
        [Column(IsNullable = false)]
        public long GroupKey { get; set; }

        [Column(IsNullable = false, StringLength = 500)]
        public required string FilePath { get; set; }

        //public int OriginalColor { get; set; }
        //public int SelectedColor { get; set; }
        //public int HoverColor { get; set; }
        /// <summary>
        /// 是否可控制
        /// </summary>
        public bool IsCanControl { get; set; }

        public ModelType Type { get; set; }

        /// <summary>
        /// 位置 示例: 50.90,42.09,93.093 (x,y,z)
        /// </summary>
        [Column(IsNullable = false, StringLength = 80)]
        public required string PositionStr { get; set; }

        /// <summary>
        /// 旋转方位 示例: 50.90,42.09,93.093 (x,y,z)
        /// </summary>
        [Column(IsNullable = false, StringLength=80)]
        public required string RotationStr { get; set; }

        /// <summary>
        /// 选装角度
        /// </summary>
        public double RotateAngle { get; set; }

        /// <summary>
        /// 放缩比例 示例: 50.90,42.09,93.093 (x,y,z)
        /// </summary>
        [Column(IsNullable = false, StringLength = 80)]
        public string ScaleStr { get; set; } = "1,1,1";

        [Column(IsNullable = true)]
        public long? MapDeviceId { get; set; }

        [Column(IsNullable = true, StringLength = 50)]
        public string? Orientation { get; set; } 

        [Column(IsNullable = true, StringLength = 200)]
        public string? Description { get; set; }

        // 显示属性
        public bool IsVisible { get; set; } = true;

        // 元数据
        public long FileSize { get; set; }
        public bool HasAnimations { get; set; }
    }
}
