using MCCS.Core.Domain.Curves;

namespace MCCS.Core.Repositories
{
    public interface ICurveAggregateRepository
    {

        Task<List<CurveAggregate>> GetCurvesAsync(CancellationToken cancellationToken = default); 
    }
}
