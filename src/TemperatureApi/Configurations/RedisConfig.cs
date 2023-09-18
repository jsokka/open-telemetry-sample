using System.ComponentModel.DataAnnotations;

namespace TemperatureApi.Configurations
{
    public class RedisConfig
    {
        public const string SectionName = "Redis";

        [Required, MinLength(1)]
        public required string Configuration { get; init; }

        [Range(1, 120)]
        public required int DefaultExpirationInMinutes { get; init; }
    }
}
