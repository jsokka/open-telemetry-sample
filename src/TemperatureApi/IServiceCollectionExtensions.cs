using Microsoft.Extensions.Primitives;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using TemperatureApi.Configurations;
using TemperatureApi.Services;

namespace TemperatureApi
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            // WeatherService
            var weatherApiConfigSection = configuration.GetSection(OpenWeatherApiConfig.SectionName);
            services.AddOptions<OpenWeatherApiConfig>()
                .Bind(weatherApiConfigSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddHttpClient<IWeatherApiClient, OpenWeatherApiClient>(opt =>
                opt.BaseAddress = new Uri(weatherApiConfigSection.Get<OpenWeatherApiConfig>()!.BaseUrl));

            // AuthenticationApi
            var authenticationConfigSection = configuration.GetSection(AuthenticationConfig.SectionName);
            services.AddOptions<AuthenticationConfig>()
                .Bind(authenticationConfigSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>(opt =>
                opt.BaseAddress = new Uri(authenticationConfigSection.Get<AuthenticationConfig>()!.ApiBaseUrl));

            return services;
        }

        public static IServiceCollection AddCache(this IServiceCollection services, ConfigurationManager configuration)
        {
            var redisConfigSection = configuration.GetSection(RedisConfig.SectionName);
            services.AddOptions<RedisConfig>()
                .Bind(redisConfigSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfigSection.Get<RedisConfig>()!.Configuration);
            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

            services.AddStackExchangeRedisCache(options =>
                options.ConnectionMultiplexerFactory = () => Task.FromResult((IConnectionMultiplexer)connectionMultiplexer));

            services.AddSingleton<ICacheClient, RedisCacheClient>();

            return services;
        }

        public static IServiceCollection AddInstrumentation(this IServiceCollection services, WebApplicationBuilder webApplicationBuilder)
        {
            var instrumentationSection = webApplicationBuilder.Configuration.GetSection(InstrumentationConfig.SectionName);
            services.AddOptions<InstrumentationConfig>()
                .Bind(instrumentationSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var otlpExportUrl = instrumentationSection.Get<InstrumentationConfig>()!.OtlpExportUrl;

            if (string.IsNullOrWhiteSpace(otlpExportUrl))
            {
                return services;
            }

            var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Environment.MachineName;
            var apiKey = instrumentationSection.Get<InstrumentationConfig>()!.ApiKey;

            var exporterOptions = (OtlpExporterOptions options) =>
            {
                options.Endpoint = new Uri(otlpExportUrl);
                options.Headers = $"api-key={apiKey}";
            };

            services.AddOpenTelemetry()
                .ConfigureResource(res =>
                    res.AddService(
                        serviceName: "TemperatureApi",
                        autoGenerateServiceInstanceId: string.IsNullOrWhiteSpace(instanceId),
                        serviceInstanceId: instanceId))
                .WithTracing(builder =>
                {
                    builder.AddAspNetCoreInstrumentation(cfg =>
                    {
                        cfg.RecordException = true;
                        cfg.EnrichWithHttpRequest = (activity, req) =>
                        {
                            if (req.Path.Value?.Equals("/temperature", StringComparison.InvariantCultureIgnoreCase) == true &&
                                req.Query.TryGetValue("q", out StringValues value))
                            {
                                activity.DisplayName += $"?q={value}";
                                activity.SetTag("temperatureApi.location", value);
                            }
                        };
                    });
                    builder.AddHttpClientInstrumentation(cfg =>
                    {
                        cfg.FilterHttpRequestMessage = (req) =>
                        {
                            return req.RequestUri?.Authority.IndexOf("applicationinsights.azure.com",
                                    StringComparison.InvariantCultureIgnoreCase) == -1 &&
                                req.RequestUri?.Authority.IndexOf("monitor.azure.com",
                                    StringComparison.InvariantCultureIgnoreCase) == -1;
                        };
                        cfg.RecordException = true;
                    });
                    builder.AddRedisInstrumentation();
                    builder.AddOtlpExporter(exporterOptions);
                })
                .WithMetrics(builder =>
                {
                    builder.AddAspNetCoreInstrumentation();
                    builder.AddHttpClientInstrumentation();
                    builder.AddProcessInstrumentation();
                    builder.AddOtlpExporter(exporterOptions);
                });

            webApplicationBuilder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.IncludeFormattedMessage = true;
                options.AddOtlpExporter(exporterOptions);
            });

            return services;
        }
    }
}
