namespace AuthenticationApi.Entities
{
    public class ApiKey
    {
        public required int Id { get; set; }

        public required string Key { get; set; }

        public virtual ICollection<ApiKeyTarget> Targets { get; set; } = new HashSet<ApiKeyTarget>();

        public virtual ICollection<AuditLogEntry> AuditLogEntries { get; set; } = new HashSet<AuditLogEntry>();
    }
}
