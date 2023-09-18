using AuthenticationApi.Entities;

namespace AuthenticationApi.Services
{
    internal enum ApiKeyValidationResult
    {
        Valid,
        ApiKeyNotFound,
        TargetNotFound
    }

    internal interface IApiKeyValidator
    {
        public Task<(ApiKeyValidationResult, ApiKey?)> ValidateApiKey(string apiKeyValue, string targetApi,
            CancellationToken cancellationToken);
    }
}
