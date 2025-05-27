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

        public int OriginalColor { get; set; }

        /// <summary>
        /// 示例: 50.90,42.09,93.093 (x,y,z)
        /// </summary>
        [Column(IsNullable = false, StringLength = 80)]
        public string PositionStr { get; set; }

        [Column(IsNullable = true, StringLength = 200)]
        public string Description { get; set; }
    }
}
