namespace MCCS.Station.Core.ValidatorRules
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public required T Data { get; set; }

        public static OperationResult<T> Success(T data = default!) => new()
        {
            IsSuccess = true,
            Data = data
        };

        public static OperationResult<T> Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            Data = default!,
            ErrorMessage = errorMessage
        };
    }
}
