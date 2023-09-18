using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi
{
    public static class WebApplicationExtensions
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting to apply migrations.");
            var dataContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
            dataContext.Database.Migrate();
        }
    }
}
