namespace TemperatureApi.Behaviors
{
    public interface ICacheableRequest
    {
        string CacheKey { get; }

        public bool EvictCache { get; init; }
    }
}
