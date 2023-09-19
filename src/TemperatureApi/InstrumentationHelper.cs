using System.Diagnostics;

namespace TemperatureApi
{
    internal static class InstrumentationHelper
    {
        internal const string ActivitySourceName = "OpenTelemetrySample.TemperatureApi";

        internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    }
}
