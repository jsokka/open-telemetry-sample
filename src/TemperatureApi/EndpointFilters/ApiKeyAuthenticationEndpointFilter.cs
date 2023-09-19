using AuthenticationApi.Utils;
using Microsoft.Extensions.Options;
using System.Diagnostics;
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
            ApiKeyValidationResult result;

            using (var activity = InstrumentationHelper.ActivitySource.StartActivity(nameof(ApiKeyAuthenticationEndpointFilter)))
            {
                activity?.AddEvent(new ActivityEvent("Start validating apiKey"));

                if (!context.HttpContext.TryGetApiKey(out string apiKey))
                {
                    _logger.LogInformation("ApiKey not provided");
                    return TypedResults.Unauthorized();
                }

                activity?.AddTag("temperatureApi.ApiKey", apiKey);

                result = await _authenticationApiClient.ValidateApiKey(apiKey, _config.TargetApi,
                    context.HttpContext.RequestAborted);

                activity?.AddEvent(new ActivityEvent("ApiKey validation done"));
            }

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
