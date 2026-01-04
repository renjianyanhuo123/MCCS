namespace MCCS.Station.ValidatorRules
{
    public interface IValidator<TContext>
    {
        OperationResult<TContext> Validate(TContext context);
    }
}
