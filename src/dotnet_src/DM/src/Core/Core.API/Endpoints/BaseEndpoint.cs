using System.Runtime.InteropServices.JavaScript;
using Core.Application.Common.Results;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Core.API.Endpoints;

public abstract class BaseEndpoint : IEndpoint
{
    public abstract void ConfigureUrlMaps(RouteGroupBuilder routeGroupBuilder, WebApplication application);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    /// <param name="result"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public ProblemHttpResult  MapResultError<TValue>(Result<TValue> result, HttpContext httpContext)
    {
        if (!result.IsFailure)
            throw new ArgumentException("Result is not failure");
        
        var (status, title, typeUrl) = result.Error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status422UnprocessableEntity, "Validation Error",
                "https://httpstatuses.com/422"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found", "https://httpstatuses.com/404"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict", "https://httpstatuses.com/409"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden", "https://httpstatuses.com/403"),
       //     ErrorType.None =>  (StatusCodes.Status520, "Bad Request - undefined error", "https://httpstatuses.com/417"),
            _ => (StatusCodes.Status500InternalServerError, "Bad Request", "https://httpstatuses.com/500"),
        };

        var extensions = new Dictionary<string, object?>(2)
        {
            ["traceId"] = httpContext.TraceIdentifier,
        };
        
        // if(!string.IsNullOrWhiteSpace(error.Code))
        //     extensions["code"] = error.Code;
        
        return TypedResults.Problem(
            statusCode: status,
            title: title,
            type: typeUrl,
            detail: result.Error.Message,
            instance: httpContext.Request.Path,
            extensions: extensions
        );
    }

    public Results<Ok<TValue>, ProblemHttpResult> ToResult<TValue>(Result<TValue> result, HttpContext httpContext)
        => result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : MapResultError(result, httpContext);
    //result.Error
}

public static class ErrorTypeMapping
{
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.None         => StatusCodes.Status200OK,
        ErrorType.BadRequest   => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden    => StatusCodes.Status403Forbidden,
        ErrorType.NotFound     => StatusCodes.Status404NotFound,
        ErrorType.Conflict     => StatusCodes.Status409Conflict,
        ErrorType.Validation   => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Failure      => StatusCodes.Status500InternalServerError,
        _                      => StatusCodes.Status500InternalServerError
    };

    public static string ToTitle(this ErrorType type) => type switch
    {
        ErrorType.BadRequest   => "Bad Request",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden    => "Forbidden",
        ErrorType.NotFound     => "Not Found",
        ErrorType.Conflict     => "Conflict",
        ErrorType.Validation   => "Validation Error",
        ErrorType.Failure      => "Unexpected Server Error",
        ErrorType.None         => "OK",
        _                      => "Unexpected Server Error"
    };
}