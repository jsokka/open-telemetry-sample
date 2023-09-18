using AuthenticationApi;
using AuthenticationApi.Contracts;
using AuthenticationApi.Entities;
using AuthenticationApi.Services;
using AuthenticationApi.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlServer<AuthenticationDbContext>(
    builder.Configuration.GetConnectionString("AuthenticationDb"));

builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

if (Uri.TryCreate(builder.Configuration.GetValue<string>("Instrumentation:OtlpExportUrl"),
    UriKind.Absolute, out Uri? otlpExportUrl))
{
    var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Environment.MachineName;
    var apiKey = builder.Configuration.GetValue<string>("Instrumentation:ApiKey");

    var exporterOptions = (OtlpExporterOptions options) =>
    {
        options.Endpoint = otlpExportUrl;
        options.Headers = $"api-key={apiKey}";
    };

    builder.Logging.AddOpenTelemetry(options =>
    {
        options.IncludeScopes = true;
        options.ParseStateValues = true;
        options.IncludeFormattedMessage = true;
        options.AddOtlpExporter(exporterOptions);
    });

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(res =>
            res.AddService(
                serviceName: "AuthenticationApi",
                autoGenerateServiceInstanceId: string.IsNullOrWhiteSpace(instanceId),
                serviceInstanceId: instanceId))
        .WithTracing(traceProviderBuilder =>
        {
            traceProviderBuilder.AddAspNetCoreInstrumentation(options => options.RecordException = true);
            traceProviderBuilder.AddSqlClientInstrumentation(options =>
            {
                options.RecordException = true;
                options.SetDbStatementForText = true;
                options.EnableConnectionLevelAttributes = true;
            });
            traceProviderBuilder.AddOtlpExporter(exporterOptions);
        })
        .WithMetrics(metricsBuilder =>
        {
            metricsBuilder.AddAspNetCoreInstrumentation();
            metricsBuilder.AddProcessInstrumentation();
            metricsBuilder.AddOtlpExporter(exporterOptions);
        });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
}

app.MapGet("api/{targetApi}", async (string targetApi, IApiKeyValidator apiKeyValidator,
    HttpContext httpContext, CancellationToken cancellationToken) =>
{
    if (!httpContext.TryGetApiKey(out string apiKey))
    {
        return Results.StatusCode(StatusCodes.Status401Unauthorized);
    }

    var (validationResult, _) = await apiKeyValidator.ValidateApiKey(apiKey, targetApi, cancellationToken);

    return ToHttpResult(validationResult);
});

app.MapPost("api/{targetApi}/audit-log", async (string targetApi, [FromBody] RequestInfo requestInfo,
    IApiKeyValidator apiKeyValidator, AuthenticationDbContext db, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    if (!httpContext.TryGetApiKey(out string headerApiKey))
    {
        return Results.StatusCode(StatusCodes.Status401Unauthorized);
    }

    var (validationResult, apiKey) = await apiKeyValidator.ValidateApiKey(headerApiKey, targetApi, cancellationToken);

    if (validationResult == ApiKeyValidationResult.Valid && apiKey is not null)
    {
        await AddAuditEntry(apiKey, targetApi, db, requestInfo, cancellationToken);
    }

    return ToHttpResult(validationResult);
});

async Task AddAuditEntry(ApiKey apiKey, string targetApi, AuthenticationDbContext db, RequestInfo requestInfo, CancellationToken cancellationToken)
{
    await db.AuditLogEntries.AddAsync(new AuditLogEntry
    {
        ApiKeyId = apiKey.Id,
        ApiKeyTargetId = apiKey.Targets.Single(t => t.Target == targetApi).Id,
        Route = requestInfo.Route,
        HttpMethod = requestInfo.HttpMethod,
        ClientIpAddress = requestInfo.ClientIpAddress,
        Timestamp = requestInfo.Timestamp,
    }, cancellationToken);

    await db.SaveChangesAsync(cancellationToken);
}

IResult ToHttpResult(ApiKeyValidationResult validationResult)
{
    return validationResult switch
    {
        ApiKeyValidationResult.ApiKeyNotFound => Results.StatusCode(StatusCodes.Status401Unauthorized),
        ApiKeyValidationResult.TargetNotFound => Results.StatusCode(StatusCodes.Status403Forbidden),
        ApiKeyValidationResult.Valid => Results.Ok(),
        _ => throw new UnreachableException(),
    };
}

app.Run();
