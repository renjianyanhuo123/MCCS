using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.Model3D
{
    [Table(Name = "tb_model3d")]
    public class Model3DData : BaseModel
    {
        [Column(IsNullable = false, StringLength = 100)]
        public string Key { get; set; }

        [Column(IsNullable = false, StringLength = 100)]
        public string Name { get; set; }

        [Column(IsNullable = false, StringLength = 50)]
        public string GroupKey { get; set; }

        [Column(IsNullable = false, StringLength = 500)]
        public string FilePath { get; set; }

        //public int OriginalColor { get; set; }
        //public int SelectedColor { get; set; }
        //public int HoverColor { get; set; }

        public ModelType Type { get; set; }

        /// <summary>
        /// 位置 示例: 50.90,42.09,93.093 (x,y,z)
        /// </summary>
        [Column(IsNullable = false, StringLength = 80)]
        public string PositionStr { get; set; }

        /// <summary>
        /// 旋转方位 示例: 50.90,42.09,93.093 (x,y,z)
        /// </summary>
        [Column(IsNullable = false, StringLength=80)]
        public string RotationStr { get; set; }

        /// <summary>
        /// 放缩比例 示例: 50.90,42.09,93.093 (x,y,z)
        /// </summary>
        [Column(IsNullable = false, StringLength = 80)]
        public string ScaleStr { get; set; } = "1,1,1";

        // 边界框信息
        public double BoundMinX { get; set; }
        public double BoundMinY { get; set; }
        public double BoundMinZ { get; set; }
        public double BoundMaxX { get; set; }
        public double BoundMaxY { get; set; }
        public double BoundMaxZ { get; set; }

        [Column(IsNullable = true, StringLength = 200)]
        public string Description { get; set; }

        // 显示属性
        public bool IsVisible { get; set; } = true;

        // 元数据
        public long FileSize { get; set; }
        public bool HasAnimations { get; set; }
    }
}
