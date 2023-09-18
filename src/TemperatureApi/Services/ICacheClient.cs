namespace TemperatureApi.Services
{
    public interface ICacheClient
    {
        Task<T?> Get<T>(string cacheKey, CancellationToken cancellationToken);

        Task Set<T>(string cacheKey, T value, CancellationToken cancellationToken);

        Task Remove(string cacheKey, CancellationToken cancellationToken);
    }
}
