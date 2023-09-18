using TemperatureApi.Models;

namespace TemperatureApi.Services
{
    public interface IWeatherApiClient
    {
        public Task<TemperatureInfo?> GetTemperatureInfo(string query, CancellationToken cancellationToken);
    }
}
