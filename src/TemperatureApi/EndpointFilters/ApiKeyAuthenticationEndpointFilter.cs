using AuthenticationApi.Utils;
using Microsoft.Extensions.Options;
using TemperatureApi.Configurations;
using TemperatureApi.Services;

namespace TemperatureApi.EndpointFilters
{
    public class ApiKeyAuthenticationEndpointFilter : IEndpointFilter
    {
        private readonly IAuthenticationApiClient _authenticationApiClient;
        private readonly AuthenticationConfig _config;
        private readonly ILogger<ApiKeyAuthenticationEndpointFilter> _logger;

        public ApiKeyAuthenticationEndpointFilter(IAuthenticationApiClient authenticationApiClient,
            IOptions<AuthenticationConfig> options,
            ILogger<ApiKeyAuthenticationEndpointFilter> logger)
        {
            _authenticationApiClient = authenticationApiClient;
            _logger = logger;
            _config = options.Value;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (!context.HttpContext.TryGetApiKey(out string apiKey))
            {
                _logger.LogInformation("ApiKey not provided");
                return TypedResults.Unauthorized();
            }

            var result = await _authenticationApiClient.ValidateApiKey(apiKey, _config.TargetApi,
                context.HttpContext.RequestAborted);

            return result switch
            {
                ApiKeyValidationResult.Authorized => await next(context),
                ApiKeyValidationResult.Unauthorized => Results.Unauthorized(),
                ApiKeyValidationResult.Forbidden => Results.StatusCode(StatusCodes.Status403Forbidden),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
