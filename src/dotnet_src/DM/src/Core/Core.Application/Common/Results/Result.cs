using System.Diagnostics.CodeAnalysis;
using Core.Domain.SharedKernel.Errors;

namespace Core.Application.Common.Results;




public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; } = Error.None;

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    protected Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }
    
    public static Result Ok()=>new (true);
    public static Result Fail(Error error)=>new(false, error);
}


public sealed class Result<T> //: Result
{
    private readonly T? _value;
    
    [MemberNotNullWhen(true, nameof(_value))]
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; } = Error.None;
   //public T Value { get; }

   public T Value => !IsSuccess || _value is null ? 
       throw new InvalidOperationException("Value is not available for failed Result.")
       : _value;
   

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    } 
    private  Result(Error error) 
    {
        if (error.Type==ErrorType.None)
            throw new ArgumentException($"{nameof(error)}.{nameof(error.Type)}", nameof(error));
       
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(Error error) => new(error);

    public bool TryGetValue([NotNullWhen(false)] out T? value)
    {
        value = _value;
        return IsSuccess;
    }
    
    
    
    // Result<int> t = t1;
   // public static implicit operator Result<T>(T value) => Ok(value);

}