using System.ComponentModel.DataAnnotations;

namespace TemperatureApi.Configurations
{
    public class AuthenticationConfig
    {
        public const string SectionName = "Authentication";

        [Url, Required]
        public required string ApiBaseUrl { get; init; }

        [Required]
        public required string TargetApi { get; init; }
    }
}
