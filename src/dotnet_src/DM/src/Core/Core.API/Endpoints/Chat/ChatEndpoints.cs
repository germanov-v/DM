
using Core.Application.Handlers.Identity.Abstractions;
using Core.Application.Handlers.Identity.Dto;
using Core.Application.Options;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Core.API.Endpoints.Chat;

public class ChatEndpoints : BaseEndpoint
{
    public override void ConfigureUrlMaps(RouteGroupBuilder routeGroupBuilder, WebApplication application)
    {
        routeGroupBuilder.MapPost("/chat/start", StubEndpoint);
        routeGroupBuilder.MapPost("/chat/messages:list", StubEndpoint);
        routeGroupBuilder.MapPost("/chat/messages:search", StubEndpoint);
        routeGroupBuilder.MapPost("/chat/send", StubEndpoint);
        routeGroupBuilder.MapPost("/chat/participants", StubEndpoint);
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