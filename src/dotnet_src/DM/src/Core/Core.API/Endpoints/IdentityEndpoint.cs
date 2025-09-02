using Core.Application.Abstractions;
using Core.Application.Dto.Identity;
using Core.Application.Handlers.Identity;
using Core.Application.Options.Identity;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.Constants;
using Core.Domain.SharedKernel.Events;
using Core.Domain.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Core.API.Endpoints;

public class IdentityEndpoint : BaseEndpoint
{
    public override void ConfigureUrlMaps(RouteGroupBuilder routeGroupBuilder, WebApplication application)
    {
        routeGroupBuilder.MapPost("/identity", Authenticate);

        routeGroupBuilder.MapGet("/test", TestDdd);

        routeGroupBuilder.MapPost("/identity/auth/moderator", WebModeratorAuthenticationByEmail);
        routeGroupBuilder.MapPost("/identity/refresh", RefreshJwtCookie);
    }
    
    public async Task<Results<Ok<AuthJwtResponse>, ProblemHttpResult>> WebModeratorAuthenticationByEmail(
        [FromBody] LoginEmailRequestDto dto,
        [FromServices] IIdentityHandler handler,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )=> await WebAuthenticationByEmail(dto, handler, RoleConstants.Moderator, httpContext, authOption, cancellationToken);

    public async Task<Results<Ok<AuthJwtResponse>, ProblemHttpResult>> WebAuthenticationByEmail(
        [FromBody] LoginEmailRequestDto dto,
        [FromServices] IIdentityHandler handler,
        string role,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
    {
     

        var resultHandler = await handler.Authenticate(dto, cancellationToken: cancellationToken);

        if (resultHandler.IsFailure)
        {
            return MapResultError(resultHandler, httpContext);
        }

        var jwtResponse = resultHandler.Value;
        var result = new AuthJwtResponse(
            jwtResponse.AccessToken,
            jwtResponse.ExpiresIn,
            jwtResponse.User
        );

        var cookieOptions = new CookieOptions()
        {
            Expires = DateTimeOffset.Now.AddSeconds(authOption.Value.RefreshTokenLifetime),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        };
        httpContext.Response.Cookies.Append(key: "refreshToken", jwtResponse.RefreshToken,
            cookieOptions);

        return TypedResults.Ok(result);
    }
    
   
    ////////////////////////////////////////////////////////////////////////////
    
    
    public async Task<IResult> Authenticate(string username, string password)
    {
        return Results.Ok(1);
    }

    public async Task<IResult> TestDdd(
        [FromServices] IUnitOfWork uow,
        [FromServices] IChangeTracker changeTracker,
        CancellationToken cancellationToken
    )
    {
        var user = new User(new IdGuid(1, Guid.NewGuid()), "test");

        user.RegisterUserByEmail("test", "test");
        changeTracker.Track(user);
        await uow.CommitTransaction(cancellationToken);
        return Results.Ok(1);
    }


   


    public async Task<Results<Ok<AuthJwtResponse>, BadRequest<ProblemHttpResult>,
        UnauthorizedHttpResult, ProblemHttpResult>> RefreshJwtCookie(
        [FromBody] RefreshTokenDto dto,
        [FromServices] IIdentityHandler handler,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
    {
        // var refreshToken = httpContext.Request.Cookies["refreshToken"] ?? String.Empty;

        if (httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            dto.RefreshToken = refreshToken;
        }

        var jwtResponse = await handler.RefreshAuth(dto, cancellationToken);
        var cookieOptions = new CookieOptions()
        {
            Expires = DateTimeOffset.Now.AddSeconds(authOption.Value.RefreshTokenLifetime),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        };
        httpContext.Response.Cookies.Append(key: "refreshToken", jwtResponse.RefreshToken,
            cookieOptions);
        var result = new AuthJwtResponse(
            jwtResponse.AccessToken,
            jwtResponse.ExpiresIn,
            jwtResponse.User
        );
        return TypedResults.Ok(result);
    }


    public async Task<Results<Ok<AuthJwtResponseDto>, BadRequest<ProblemHttpResult>>> AuthenticationByEmail(
        [FromBody] LoginEmailRequestDto dto,
        [FromServices] IIdentityHandler handler,
        string role,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
        // var roleDto = new LoginEmailRoleFingerprintRequestDto(dto.Email, dto.Password, role, dto.Fingerprint);
        //
        //
        // var result = await handler.Authenticate(roleDto, cancellationToken: cancellationToken);
        //
        // return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<AuthJwtResponseDto>, BadRequest<ProblemHttpResult>,
        UnauthorizedHttpResult, ProblemHttpResult>> RefreshJwt(
        [FromBody] RefreshTokenDto dto,
        [FromServices] IIdentityHandler handler,
        CancellationToken cancellationToken
    ) => TypedResults.Ok(await handler.RefreshAuth(dto, cancellationToken));
}