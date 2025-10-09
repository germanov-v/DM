using Core.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Core.API.Endpoints.Company;

public class ProfileEndpoint : BaseEndpoint
{
    public override void ConfigureUrlMaps(RouteGroupBuilder routeGroupBuilder, WebApplication application)
    {
       

        routeGroupBuilder.MapPost("/company/start", StartProvider);
        
        routeGroupBuilder.MapGet("/company/about", AboutGet);
        routeGroupBuilder.MapPost("/company/about", AboutEdit);

    }

    [Authorize(Policy = IdentityAuthConstants.AuthPolicyCompany)]
    public async Task<Results<Ok<bool>, ProblemHttpResult>> StartProvider()
    {
        
        
        return TypedResults.Ok(true);
    }
    
    [Authorize(Policy = IdentityAuthConstants.AuthPolicyCompany)]
    public async Task<Results<Ok<int>, ProblemHttpResult>> AboutGet()
    {
        
        
        return TypedResults.Ok(int.MaxValue);
    }
    
    [Authorize(Policy = IdentityAuthConstants.AuthPolicyCompany)]
    public async Task<Results<Ok<bool>, ProblemHttpResult>> AboutEdit()
    {
        
        
        return TypedResults.Ok(true);
    }
    
}