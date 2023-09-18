using AuthenticationApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Services
{
    internal class ApiKeyValidator : IApiKeyValidator
    {
        private readonly AuthenticationDbContext db;
        private readonly ILogger<ApiKeyValidator> logger;

        public ApiKeyValidator(AuthenticationDbContext db, ILogger<ApiKeyValidator> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task<(ApiKeyValidationResult, ApiKey?)> ValidateApiKey(string apiKeyValue, string targetApi,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting to validate api key");

            if (string.IsNullOrWhiteSpace(apiKeyValue))
            {
                logger.LogWarning("Api key is null");
                return (ApiKeyValidationResult.ApiKeyNotFound, null);
            }

            var apiKey = await GetApiKey(apiKeyValue, cancellationToken);

            if (apiKey is null)
            {
                logger.LogWarning("Api key not found in db");
                return (ApiKeyValidationResult.ApiKeyNotFound, null);
            }

            if (!apiKey.Targets.Any(t => t.Target == targetApi))
            {
                logger.LogWarning("Api key has no configured access to target api {targetApi}", targetApi);
                return (ApiKeyValidationResult.TargetNotFound, apiKey);
            }

            logger.LogInformation("Api key is valid");

            return (ApiKeyValidationResult.Valid, apiKey);
        }

        private async Task<ApiKey?> GetApiKey(string apiKeyHeaderValue, CancellationToken cancellationToken)
        {
            return await db.ApiKeys.Include(a => a.Targets)
                .FirstOrDefaultAsync(a => a.Key == apiKeyHeaderValue, cancellationToken);
        }
    }
}
