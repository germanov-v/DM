
using Core.Application.Handlers.Identity.Abstractions;
using Core.Application.Handlers.Identity.Dto;
using Core.Application.Options;
using Core.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Core.API.Endpoints.Identity;

public class EmailIdentityEndpoint : BaseEndpoint
{
    public override void ConfigureUrlMaps(RouteGroupBuilder routeGroupBuilder, WebApplication application)
    {
        // routeGroupBuilder.MapPost("/identity", Authenticate);
        //
        // routeGroupBuilder.MapGet("/test", TestDdd);

        routeGroupBuilder.MapPost("/identity/auth/moderator", WebModeratorAuthenticationByEmail);
        routeGroupBuilder.MapPost("/identity/refresh", WebRefreshJwtCookie);
        
        routeGroupBuilder.MapPost("/identity/auth/company", WebProviderAuthenticationByEmail);
        routeGroupBuilder.MapPost("/identity/auth/company/manager", WebModeratorAuthenticationByEmail);

    }
    
    
    public async Task<Results<Ok<AuthJwtResponse>, ProblemHttpResult>> WebProviderAuthenticationByEmail(
        [FromBody] LoginEmailRequestDto dto,
        [FromServices] IIdentityHandler handler,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
        => await WebAuthenticationByEmail(dto, handler, RoleConstants.Company, httpContext, authOption, cancellationToken);
    
    public async Task<Results<Ok<AuthJwtResponse>, ProblemHttpResult>> WebManagerProviderAuthenticationByEmail(
        [FromBody] LoginEmailRequestDto dto,
        [FromServices] IIdentityHandler handler,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
        => await WebAuthenticationByEmail(dto, handler, RoleConstants.ManagerCompany, httpContext, authOption, cancellationToken);


    
    public async Task<Results<Ok<AuthJwtResponse>, ProblemHttpResult>> WebModeratorAuthenticationByEmail(
        [FromBody] LoginEmailRequestDto dto,
        [FromServices] IIdentityHandler handler,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
        => await WebAuthenticationByEmail(dto, handler, RoleConstants.Moderator, httpContext, authOption, cancellationToken);

    public async Task<Results<Ok<AuthJwtResponse>, ProblemHttpResult>> WebAuthenticationByEmail(
        [FromBody] LoginEmailRequestDto dto,
        [FromServices] IIdentityHandler handler,
        string role,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
    {
     

        var resultHandler = await handler.AuthenticateByEmailPasswordRole(dto.Email,
            dto.Password,
            role,
            httpContext.Connection.RemoteIpAddress,
            String.Empty, 
            
            cancellationToken: cancellationToken);

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
    
    
    public async Task<Results<Ok<AuthJwtResponse>, BadRequest<ProblemHttpResult>,
        UnauthorizedHttpResult, ProblemHttpResult>> WebRefreshJwtCookie(
        [FromBody] RefreshTokenDto dto,
        [FromServices] IIdentityHandler handler,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
    {
       
        if (httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            dto.RefreshToken = refreshToken;
        }

        var resultHandler = await handler.RefreshAuth(dto.RefreshToken, httpContext.Connection.RemoteIpAddress, cancellationToken);
        
        if (resultHandler.IsFailure)
        {
            return MapResultError(resultHandler, httpContext);
        }
        
        var cookieOptions = new CookieOptions()
        {
            Expires = DateTimeOffset.Now.AddSeconds(authOption.Value.RefreshTokenLifetime),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        };
        httpContext.Response.Cookies.Append(key: "refreshToken", resultHandler.Value.RefreshToken,
            cookieOptions);
        var result = new AuthJwtResponse(
            resultHandler.Value.AccessToken,
            resultHandler.Value.ExpiresIn,
            resultHandler.Value.User
        );
        return TypedResults.Ok(result);
    }

    
   
    ////////////////////////////////////////////////////////////////////////////
    
    
    // public async Task<IResult> Authenticate(string username, string password)
    // {
    //     return Results.Ok(1);
    // }
    //
    // public async Task<IResult> TestDdd(
    //     [FromServices] IUnitOfWork uow,
    //     [FromServices] IChangeTracker changeTracker,
    //     CancellationToken cancellationToken
    // )
    // {
    //     var user = new User(new IdGuid(1, Guid.NewGuid()), "test");
    //
    //     user.RegisterUserByEmail("test", "test");
    //     changeTracker.Track(user);
    //     await uow.CommitTransaction(cancellationToken);
    //     return Results.Ok(1);
    // }


   


   

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
    ) =>   throw new NotImplementedException();// TypedResults.Ok(await handler.RefreshAuth(dto, cancellationToken));
}