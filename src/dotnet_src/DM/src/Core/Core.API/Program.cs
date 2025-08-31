using System.Reflection;
using Core.API.Extensions.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddEventHandlers();

var app = builder.Build();



app.MapGet("/", () => "Hello World!");

var apiVersion = app.MapGroup("/api/v1");
app.AddMapsConfigure(apiVersion, [Assembly.GetExecutingAssembly()]);

app.Run();