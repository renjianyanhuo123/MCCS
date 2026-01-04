namespace MCCS.Station.Core.ValidatorRules
{
    public interface IValidator<TContext>
    {
        OperationResult<TContext> Validate(TContext context);
    }
}
