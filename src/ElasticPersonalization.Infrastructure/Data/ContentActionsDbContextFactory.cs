using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ElasticPersonalization.Infrastructure.Data
{
    public class ContentActionsDbContextFactory : IDesignTimeDbContextFactory<ContentActionsDbContext>
    {
        public ContentActionsDbContext CreateDbContext(string[] args)
        {
            // Get environment
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Build configuration
            IConfigurationRoot configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("ContentActionsConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Use a default connection string if not found in configuration
                connectionString = "Server=localhost;Database=ContentActions;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;";
            }

            // Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<ContentActionsDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ContentActionsDbContext(optionsBuilder.Options);
        }
    }
}
