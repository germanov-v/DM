using System.Reflection;
using Core.API.Endpoints;

namespace Core.API.Extensions.DI;

/// <summary>
/// https://learn.microsoft.com/ru-ru/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0#exception-handler-lambda
///
/// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0
/// </summary>
public static class EndpointsExtension
{
	public static WebApplication AddMapsConfigure(this WebApplication webApplication,
		RouteGroupBuilder routeGroupBuilder,
		Assembly[] assemblies)
	{


		var classes =
			assemblies
				.SelectMany(p =>
					p.GetTypes().Where(type =>
						type is
						{
							IsClass: true,
							IsAbstract: false
						} &&
						typeof(IEndpoint).IsAssignableFrom(type)
					));

		foreach (var item in classes)
		{

			var endpoint = (IEndpoint)Activator.CreateInstance(item)!;
			endpoint.ConfigureUrlMaps(routeGroupBuilder,webApplication);
		}


		return webApplication;
	}




}
