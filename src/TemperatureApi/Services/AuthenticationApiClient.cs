using AuthenticationApi.Contracts;
using AuthenticationApi.Utils;
using System.Net;

namespace TemperatureApi.Services
{
    public class AuthenticationApiClient : IAuthenticationApiClient
    {
        private readonly HttpClient httpClient;

        public AuthenticationApiClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ApiKeyValidationResult> ValidateApiKey(string apiKey, string targetApi, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/{targetApi}");
            request.Headers.Add(Constants.ApiKeyHeaderName, apiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);

            return response?.StatusCode switch
            {
                HttpStatusCode.OK => ApiKeyValidationResult.Authorized,
                HttpStatusCode.Forbidden => ApiKeyValidationResult.Forbidden,
                HttpStatusCode.Unauthorized => ApiKeyValidationResult.Unauthorized,
                _ => ApiKeyValidationResult.ValidationFailed
            };
        }

        public async Task<bool> LogRequest(string apiKey, string targetApi, RequestInfo requestInfo, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/{targetApi}/audit-log");
            request.Headers.Add(Constants.ApiKeyHeaderName, apiKey);
            request.Content = JsonContent.Create(requestInfo);

            var response = await httpClient.SendAsync(request, cancellationToken);

            return response?.StatusCode switch
            {
                HttpStatusCode.OK => true,
                _ => false
            };
        }
    }
}
