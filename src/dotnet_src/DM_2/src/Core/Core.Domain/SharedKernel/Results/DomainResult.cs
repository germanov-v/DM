namespace Core.Domain.SharedKernel.Results;

public class DomainResult
{
    public bool IsSuccess { get; }
    public DomainError Error { get; }

    protected DomainResult(bool isSuccess, DomainError? error)
    {
        IsSuccess = isSuccess;
        Error = error??new DomainError();
    }

    public static DomainResult Ok() => new(true, null);
    public static DomainResult Fail(DomainError error) => new(false, error);
}


public class DomainResult<T> : DomainResult
{
   
    public T? Value { get; }
  

    private DomainResult(T value) : base (true, null)
    {
    
        Value = value;
    
    }

    private DomainResult(DomainError error) : base (false, error)
    {
       
        Value = default;
    }

    public static DomainResult<T> Ok(T value) => new(value);
    public new static  DomainResult<T> Fail(DomainError error) => new(error);
}