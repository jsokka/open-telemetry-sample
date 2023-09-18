using System.ComponentModel.DataAnnotations;

namespace TemperatureApi.Configurations
{
    public class OpenWeatherApiConfig
    {
        public const string SectionName = "OpenWeatherApi";

        [Url]
        public string BaseUrl { get; set; } = null!;

        [Required, StringLength(32, MinimumLength = 32)]
        public string ApiKey { get; set; } = null!;
    }
}
