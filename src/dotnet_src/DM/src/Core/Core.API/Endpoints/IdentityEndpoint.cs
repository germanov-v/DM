using Core.Application.Abstractions;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Events;
using Core.Domain.SharedKernel.Results;
using Core.Domain.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Core.API.Endpoints;

public class IdentityEndpoint : IEndpoint
{
    public void ConfigureUrlMaps(RouteGroupBuilder routeGroupBuilder, WebApplication application)
    {
        routeGroupBuilder.MapPost("/identity",  Authenticate);

        routeGroupBuilder.MapGet("/test",   TestDdd);
    }
    
    public async Task<IResult> Authenticate(string username, string password)
    {
        
        return Results.Ok(1);
    }

    public async Task<IResult> TestDdd(
          [FromServices] IUnitOfWork uow,
          [FromServices] IChangeTracker  changeTracker,
          CancellationToken cancellationToken
        )
    {
        var user = new User(new IdGuid(1, Guid.NewGuid()), "test");
        
        user.RegisterUserByEmail("test","test");
        changeTracker.Track(user);
        await uow.CommitTransaction(cancellationToken);
        return Results.Ok(1);
    }

   
}