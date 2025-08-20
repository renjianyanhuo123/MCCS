using FreeSql.DataAnnotations;

namespace MCCS.Core.Models.Model3D
{
    [Table(Name = "tb_model3dBaseInfo")]
    public class Model3DBaseInfo : BaseModel
    {
        /// <summary>
        /// 当前模型整体的名称
        /// </summary>
        [Column(StringLength = 100, IsNullable = false)]
        public required string Name { get; set; }

        /// <summary>
        /// 模型整体是否正在使用
        /// </summary>
        public bool IsUse { get; set; }

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
    }
}
