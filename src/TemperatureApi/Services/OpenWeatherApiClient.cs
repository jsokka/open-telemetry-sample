using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net;
using TemperatureApi.Configurations;
using TemperatureApi.Models;

namespace TemperatureApi.Services
{
    public class OpenWeatherApiClient : IWeatherApiClient
    {
        private readonly HttpClient httpClient;
        private readonly IDictionary<string, string> defaultQueryParameters;

        public OpenWeatherApiClient(HttpClient httpClient, IOptions<OpenWeatherApiConfig> options)
        {
            this.httpClient = httpClient;

            defaultQueryParameters = new Dictionary<string, string>
            {
                { "units", "metric" },
                { "appid", options.Value.ApiKey }
            };
        }

        public async Task<TemperatureInfo?> GetTemperatureInfo(string query, CancellationToken cancellationToken)
        {
            var url = GetRequestUrl("weather", new Dictionary<string, string>
            {
                { "q", $"{query}".Trim() }
            });

            try
            {
                return await httpClient.GetFromJsonAsync<TemperatureInfo>(url,
                    cancellationToken: cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private string GetRequestUrl(string path, Dictionary<string, string> customParameters)
        {
            var parameters = customParameters.ToDictionary(p => p.Key, p => p.Value);

            foreach (var p in defaultQueryParameters)
            {
                if (!parameters.ContainsKey(p.Key))
                {
                    parameters.Add(p.Key, p.Value);
                }
            }

            return QueryHelpers.AddQueryString(path, parameters!);
        }
    }
}
