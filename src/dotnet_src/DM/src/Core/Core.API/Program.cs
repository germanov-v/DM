using System.Reflection;
using Core.API.Extensions.DI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer(); // Minimal API
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddEventHandlers();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

var apiVersion = app.MapGroup("/api/v1")
    .WithOpenApi(operation =>
    {
        operation.Responses.TryAdd("400", new Microsoft.OpenApi.Models.OpenApiResponse { Description = "Request validation error" });
        operation.Responses.TryAdd("422", new Microsoft.OpenApi.Models.OpenApiResponse { Description = "Validation error" });
        operation.Responses.TryAdd("404", new Microsoft.OpenApi.Models.OpenApiResponse { Description = "Resource not found" });
        operation.Responses.TryAdd("409", new Microsoft.OpenApi.Models.OpenApiResponse { Description = "Conflict" });
        operation.Responses.TryAdd("403", new Microsoft.OpenApi.Models.OpenApiResponse { Description = "Forbidden" });
        operation.Responses.TryAdd("500", new Microsoft.OpenApi.Models.OpenApiResponse { Description = "Unexpected server error" });
        
        
        return operation;
    });
app.AddMapsConfigure(apiVersion, [Assembly.GetExecutingAssembly()]);

app.Run();