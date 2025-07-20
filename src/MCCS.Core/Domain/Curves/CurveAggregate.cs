using MCCS.Core.Models.CurveModels;

namespace MCCS.Core.Domain.Curves
{
    public class CurveAggregate(CurveInfo curveInfo, AxisEntity xAxisEntity, List<AxisEntity> yAxisEntities)
    {
        public CurveInfo CurveInfo { get; private set; } = curveInfo;

        public AxisEntity XAxisInfoEntity { get; private set; } = xAxisEntity ?? throw new ArgumentNullException(nameof(xAxisEntity));

        public List<AxisEntity> YAxisInfoEntity { get; private set; } = yAxisEntities ?? throw new ArgumentNullException(nameof(yAxisEntities));
    }
}
