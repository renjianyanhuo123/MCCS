namespace MCCS.Collecter.ValidatorRules
{
    public interface IValidator<TContext>
    {
        OperationResult<TContext> Validate(TContext context);
    }
}
