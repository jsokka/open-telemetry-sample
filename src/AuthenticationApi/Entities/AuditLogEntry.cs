namespace AuthenticationApi.Entities
{
    public class AuditLogEntry
    {
        public int Id { get; set; }

        public required DateTimeOffset Timestamp { get; set; }

        public required int ApiKeyId { get; set; }

        public required int ApiKeyTargetId { get; set; }

        public required string HttpMethod { get; set; }

        public required string Route { get; set; }

        public required string ClientIpAddress { get; set; }

        public virtual ApiKey ApiKey { get; set; } = null!;

        public virtual ApiKeyTarget Target { get; set; } = null!;
    }
}
