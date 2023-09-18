using AuthenticationApi.Contracts;

namespace TemperatureApi.Services
{
    public enum ApiKeyValidationResult
    {
        Authorized,
        Unauthorized,
        Forbidden,
        ValidationFailed
    }

    public interface IAuthenticationApiClient
    {
        public Task<ApiKeyValidationResult> ValidateApiKey(string apiKey, string targetApi,
            CancellationToken cancellationToken);

        public Task<bool> LogRequest(string apiKey, string targetApi,
            RequestInfo requestInfo, CancellationToken cancellationToken);
    }
}
