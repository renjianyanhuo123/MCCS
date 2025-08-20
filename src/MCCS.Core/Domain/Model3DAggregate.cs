using MCCS.Core.Models.Model3D;

namespace MCCS.Core.Domain
{
    public class Model3DAggregate
    {
        public Model3DBaseInfo BaseInfo { get; set; }

        public List<Model3DData> Model3DDataList { get; set; }
    }
}
