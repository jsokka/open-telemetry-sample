using MediatR;
using TemperatureApi.Behaviors;
using TemperatureApi.Models;
using TemperatureApi.Services;

namespace TemperatureApi.Handlers
{
    public class GetTemperatureHandler : IRequestHandler<GetTemperatureHandler.TemperatureQuery, TemperatureInfo?>
    {
        private readonly IWeatherApiClient weatherService;

        public GetTemperatureHandler(IWeatherApiClient weatherService)
        {
            this.weatherService = weatherService;
        }

        public async Task<TemperatureInfo?> Handle(TemperatureQuery request, CancellationToken cancellationToken)
        {
            return await weatherService.GetTemperatureInfo(request.Query, cancellationToken);
        }

        public class TemperatureQuery : IRequest<TemperatureInfo>, ICacheableRequest
        {
            public required string Query { get; init; }

            public bool EvictCache { get; init; }

            public string CacheKey => $"temperature:{Query}";
        }
    }
}
