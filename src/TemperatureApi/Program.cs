using MediatR;
using Microsoft.AspNetCore.Mvc;
using TemperatureApi;
using TemperatureApi.Behaviors;
using TemperatureApi.EndpointFilters;
using TemperatureApi.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ResponseCacheBehavior<,>));
});

builder.Services
    .AddServices(builder.Configuration)
    .AddCache(builder.Configuration)
    .AddInstrumentation(builder);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("temperature", async (string q, bool? evictCache,
    [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var query = new GetTemperatureHandler.TemperatureQuery
    {
        Query = q,
        EvictCache = evictCache ?? false
    };

    var temperatureInfo = await mediator.Send(query, cancellationToken);

    if (temperatureInfo is null)
    {
        return Results.NotFound();
    }

    return TypedResults.Ok(temperatureInfo);
}).AddEndpointFilter<ApiKeyAuthenticationEndpointFilter>()
.AddEndpointFilter<AuditLogEndpointFilter>();

app.Run();