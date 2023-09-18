using System.Text.Json.Serialization;

namespace TemperatureApi.Models
{
    public class TemperatureInfo
    {
        [JsonPropertyName("main")]
        public required TemperatureData Data { get; init; }

        [JsonPropertyName("coord")]
        public required Coordinates Coordinates { get; init; }

        [JsonPropertyName("dt")]
        public required int EpochTimestamp { get; init; }

        [JsonPropertyName("timezone")]
        public required int TimeZone { get; init; }

        [JsonPropertyName("timestamp_utc")]
        public DateTime TimestampUtc => DateTimeOffset.FromUnixTimeSeconds(EpochTimestamp).UtcDateTime;

        [JsonPropertyName("timestamp_local")]
        public DateTime TimestampLocal => DateTimeOffset.FromUnixTimeSeconds(EpochTimestamp + TimeZone).LocalDateTime;
    }

    public class Coordinates
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }
    }

    public class TemperatureData
    {
        [JsonPropertyName("temp")]
        public decimal Temperature { get; set; }

        [JsonPropertyName("feels_like")]
        public decimal FeelsLike { get; set; }

        [JsonPropertyName("temp_min")]
        public decimal TemperatureMin { get; set; }

        [JsonPropertyName("temp_max")]
        public decimal TemperatureMax { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }
    }
}
