namespace MCCS.WorkflowSetting.Serialization.Dtos
{
    /// <summary>
    /// 点坐标序列化数据传输对象
    /// </summary>
    public class PointDto
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y坐标
        /// </summary>
        public double Y { get; set; }

        public PointDto()
        {
        }

        public PointDto(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
