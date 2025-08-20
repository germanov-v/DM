namespace Core.Domain.SharedKernel.Results;


public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Failure
}

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure);

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Ok()=>new (true, null);
    public static Result Fail(Error error)=>new(false, error);
}


public class Result<T> : Result
{
    public T? Value { get; }
    
    
    private  Result(T? value) : base(true, null) => Value = value;
    private  Result(Error error) : base(false, error) { }

    public static Result<T> Ok(T value) => new(value);
    public new static Result<T> Fail(Error error) => new(error);
    
    public static implicit operator Result<T>(T value) => Ok(value);

}