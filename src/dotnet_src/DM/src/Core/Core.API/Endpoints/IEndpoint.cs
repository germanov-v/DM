namespace Core.API.Endpoints;

public interface IEndpoint
{
    public void ConfigureUrlMaps(RouteGroupBuilder routeGroupBuilder, WebApplication application);
}