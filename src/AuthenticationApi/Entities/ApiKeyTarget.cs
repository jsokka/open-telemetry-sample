namespace AuthenticationApi.Entities
{
    public class ApiKeyTarget
    {
        public required int Id { get; set; }

        public required int ApiKeyId { get; set; }

        public required string Target { get; set; }

        public virtual ICollection<AuditLogEntry> AuditLogEntries { get; set; } = new HashSet<AuditLogEntry>();
    }
}
