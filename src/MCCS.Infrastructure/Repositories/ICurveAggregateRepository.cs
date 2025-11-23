using MCCS.Infrastructure.Domain.Curves;

namespace MCCS.Infrastructure.Repositories
{
    public interface ICurveAggregateRepository
    {

        Task<List<CurveAggregate>> GetCurvesAsync(CancellationToken cancellationToken = default); 
    }
}
