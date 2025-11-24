using MCCS.Infrastructure.DbContexts;
using MCCS.Infrastructure.Domain.Curves;
using MCCS.Infrastructure.Models.CurveModels;

namespace MCCS.Infrastructure.Repositories
{
    public class CurveAggregateRepository(IFreeSql<SystemDbFlag> freeSql) : ICurveAggregateRepository
    {
        public async Task<List<CurveAggregate>> GetCurvesAsync(CancellationToken cancellationToken = default)
        {
            var curveInfos = await freeSql.Select<CurveInfo>()
                .Where(c => true)
                .ToListAsync(cancellationToken);
            var axisEntities = await freeSql.Select<AxisInfo>()
                .Where(a => true)
                .ToListAsync(cancellationToken);
            var res = (from curveInfo in curveInfos
                let xAxis = axisEntities.FirstOrDefault(c => c.CurveId == curveInfo.Id && c.AxisType == AxisTypeEnum.X) ?? throw new ArgumentNullException("xAxis count < 1")
                let xAxisEntity = new AxisEntity
                {
                    AxisName = xAxis.AxisName,
                    CurveId = xAxis.Id,
                    IsAutoScale = xAxis.IsAutoScale,
                    MaxLimit = xAxis.MaxLimit,
                    MinLimit = xAxis.MinLimit,
                    Unit = xAxis.Unit,
                    VariableId = xAxis.VariableId
                }
                let yAxiss = axisEntities.Where(c => c.CurveId == curveInfo.Id && c.AxisType == AxisTypeEnum.Y)
                    .Select(s => new AxisEntity
                    {
                        AxisName = s.AxisName,
                        CurveId = s.Id,
                        IsAutoScale = s.IsAutoScale,
                        MaxLimit = s.MaxLimit,
                        MinLimit = s.MinLimit,
                        Unit = s.Unit,
                        VariableId = s.VariableId
                    })
                    .ToList()
                select new CurveAggregate(curveInfo, xAxisEntity, yAxiss)).ToList();
            return res;
        }
    }
}
