using MCCS.Infrastructure.Models.Model3D;

namespace MCCS.Infrastructure.Domain
{
    public class Model3DAggregate
    {
        public Model3DBaseInfo? BaseInfo { get; set; }

        public List<Model3DData> Model3DDataList { get; set; } = [];
        /// <summary>
        /// 所有的广告牌信息
        /// </summary>
        public List<ModelBillboardInfo> BillboardInfos { get; set; } = [];
    }
}
