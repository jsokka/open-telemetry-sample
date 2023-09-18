using AuthenticationApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi
{
    public class AuthenticationDbContext : DbContext
    {
        public AuthenticationDbContext(DbContextOptions dbContextOptions): base(dbContextOptions) { }

        public DbSet<ApiKey> ApiKeys { get; set; }

        public DbSet<AuditLogEntry> AuditLogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLogEntry>(e =>
            {
                e.Property(e => e.Id).UseIdentityColumn();
                e.HasOne(e => e.ApiKey)
                    .WithMany(a => a.AuditLogEntries)
                    .OnDelete(DeleteBehavior.NoAction);
                e.HasOne(e => e.Target)
                    .WithMany(t => t.AuditLogEntries)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Seed data
            modelBuilder.Entity<ApiKey>()
                .HasData(
                    new ApiKey
                    {
                        Id = 1,
                        Key = "DD725D0D9DBB43479B760A0F1C95D43D"
                    });

            modelBuilder.Entity<ApiKeyTarget>()
                .HasData(
                    new ApiKeyTarget
                    {
                        Id = 1,
                        ApiKeyId = 1,
                        Target = "TemperatureApi"
                    }
                );
        }
    }
}
