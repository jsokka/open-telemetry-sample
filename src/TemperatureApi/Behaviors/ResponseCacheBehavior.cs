using MediatR;
using TemperatureApi.Services;

namespace TemperatureApi.Behaviors
{
    public class ResponseCacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, ICacheableRequest
    {
        private readonly ICacheClient cacheClient;
        private readonly ILogger<ResponseCacheBehavior<TRequest, TResponse>> logger;

        public ResponseCacheBehavior(ICacheClient cacheClient, ILogger<ResponseCacheBehavior<TRequest, TResponse>> logger)
        {
            this.cacheClient = cacheClient;
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is ICacheableRequest cacheableRequest)
            {
                logger.LogInformation("Request is {IHasResponseCache} ({requestType})",
                    nameof(ICacheableRequest), request.GetType().Name);

                TResponse? response;

                if (!cacheableRequest.EvictCache)
                {
                    response = await cacheClient.Get<TResponse>(cacheableRequest.CacheKey, cancellationToken);

                    if (response is not null)
                    {
                        return response;
                    }
                }

                response = await next();

                if (response is not null)
                {
                    await cacheClient.Set(cacheableRequest.CacheKey, response, cancellationToken);
                }

                return response;
            }

            return await next();
        }
    }
}
