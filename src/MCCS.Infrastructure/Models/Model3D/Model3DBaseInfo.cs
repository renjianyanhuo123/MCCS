using FreeSql.DataAnnotations;

namespace MCCS.Infrastructure.Models.Model3D
{
    [Table(Name = "tb_model3dBaseInfo")]
    public class Model3DBaseInfo : BaseModel
    {
        /// <summary>
        /// 当前模型整体的名称
        /// </summary>
        [Column(StringLength = 100, IsNullable = false)]
        public required string Name { get; set; }

        [Column(StringLength = 500, IsNullable = true)]
        public string? Description { get; set; }

        [Column(StringLength = 50, IsNullable = false)]
        public required string MaterialColor { get; set; }

        /// <summary>
        /// 相机背景色
        /// </summary>
        [Column(StringLength = 20, IsNullable = false)]
        public string CameraBackgroundColor { get; set; }

        /// <summary>
        /// 站点ID
        /// </summary>
        public long StationId { get; set; }

        /// <summary>
        /// 相机的位置
        /// </summary>
        [Column(StringLength = 50, IsNullable = true)]
        public string CameraPosition { get; set; }
        /// <summary>
        /// 相机的朝向方向
        /// </summary>
        [Column(StringLength = 50, IsNullable = true)]
        public string CameraLookDirection { get; set; }
        /// <summary>
        /// 相机的上方向
        /// </summary>
        [Column(StringLength = 50, IsNullable = true)]
        public string CameraUpDirection { get; set; }

        /// <summary>
        /// 近裁剪平面
        /// </summary>
        public double NearPlaneDistance { get; set; }
        /// <summary>
        /// 远裁剪平面
        /// </summary>
        public double FarPlaneDistance { get; set; }
        /// <summary>
        /// 视野宽度
        /// </summary>
        public double FieldViewWidth { get; set; }
    }
}
