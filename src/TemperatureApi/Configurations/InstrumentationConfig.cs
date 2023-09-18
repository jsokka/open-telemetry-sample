using System.ComponentModel.DataAnnotations;

namespace TemperatureApi.Configurations
{
    public class InstrumentationConfig
    {
        public const string SectionName = "Instrumentation";

        [Url]
        public string? OtlpExportUrl { get; set; }

        public string? ApiKey { get; set; }
    }
}
