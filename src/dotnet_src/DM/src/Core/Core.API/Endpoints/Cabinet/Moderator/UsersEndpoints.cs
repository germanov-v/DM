using Core.Application.Dto.Filter;
using Core.Application.Handlers.Cabinet.Moderator.Users.Dto;
using Core.Application.Handlers.Identity.Abstractions;
using Core.Application.Handlers.Identity.Dto;
using Core.Application.Options;
using Core.Domain.BoundedContext.Identity.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Core.API.Endpoints.Cabinet.Moderator;

public sealed class UsersEndpoints  : BaseEndpoint
{
    public override void ConfigureUrlMaps(RouteGroupBuilder routeGroupBuilder, WebApplication application)
    {
       
        routeGroupBuilder.MapPost("/moderator/users:search", StubEndpoint);
        routeGroupBuilder.MapGet("/moderator/users/{id}", StubEndpoint);
        routeGroupBuilder.MapPost("/moderator/users", StubEndpoint);
        
        routeGroupBuilder.MapPatch("/moderator/users/{id}", StubEndpoint);
        routeGroupBuilder.MapPut("/moderator/users/{id}", StubEndpoint);
        routeGroupBuilder.MapDelete("/moderator/users/{id}", StubEndpoint);
        
        routeGroupBuilder.MapGet("/moderator/roles", StubEndpoint);
      //  routeGroupBuilder.MapPost("/identity/auth/company", StubEndpoint);
      //  routeGroupBuilder.MapPost("/identity/auth/company/manager", StubEndpoint);

    }

    
    public async Task<Results<Ok<PaginatedResult<User>>, ProblemHttpResult>> Search(
        [FromBody] PaginatedQuery<UserFilter> dto,
        [FromServices] IIdentityHandler handler,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
     //   return TypedResults.Ok(1L);
    }
    
    public async Task<Results<Ok<long>, ProblemHttpResult>> StubEndpoint(
        [FromBody] LoginEmailRequestDto dto,
        [FromServices] IIdentityHandler handler,
        HttpContext httpContext,
        IOptions<IdentityAuthOptions> authOption,
        CancellationToken cancellationToken
    )
    {
        return TypedResults.Ok(1L);
    }
}