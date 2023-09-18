using AuthenticationApi.Contracts;
using AuthenticationApi.Utils;
using Microsoft.Extensions.Options;
using TemperatureApi.Configurations;
using TemperatureApi.Services;

namespace TemperatureApi.EndpointFilters
{
    public class AuditLogEndpointFilter : IEndpointFilter
    {
        private readonly IAuthenticationApiClient _authenticationApiClient;
        private readonly ILogger<AuditLogEndpointFilter> _logger;
        private readonly AuthenticationConfig _config;

        public AuditLogEndpointFilter(IAuthenticationApiClient authenticationApiClient,
            IOptions<AuthenticationConfig> options, ILogger<AuditLogEndpointFilter> logger)
        {
            _authenticationApiClient = authenticationApiClient;
            _logger = logger;
            _config = options.Value;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var requestInfo = new RequestInfo
            {
                Route = context.HttpContext.Request.Path,
                ClientIpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?",
                HttpMethod = context.HttpContext.Request.Method,
                Timestamp = DateTimeOffset.UtcNow,
            };

            if (!context.HttpContext.TryGetApiKey(out string apiKey))
            {
                _logger.LogInformation("ApiKey not provided");
                return Results.Unauthorized();
            }

            var auditSuccess = await _authenticationApiClient.LogRequest(apiKey, _config.TargetApi, requestInfo,
                context.HttpContext.RequestAborted);

            if (!auditSuccess)
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            return await next(context);
        }
    }
}
