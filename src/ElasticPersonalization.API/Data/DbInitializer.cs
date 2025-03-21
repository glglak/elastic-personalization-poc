using ElasticPersonalization.Core.Entities;
using ElasticPersonalization.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElasticPersonalization.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ContentActionsDbContext>();
                
                // Ensure database is created
                context.Database.EnsureCreated();
                
                // Add seed data if the database is empty
                SeedData(context, logger);
                
                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
            }
        }
        
        private static void SeedData(ContentActionsDbContext context, ILogger logger)
        {
            // Check if data already exists
            if (context.Users.Any())
            {
                logger.LogInformation("Database already contains data, skipping seed");
                return;
            }
            
            logger.LogInformation("Seeding database with initial data");
            
            // Add sample users
            var users = new List<User>
            {
                new User
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    Preferences = new List<string> { "tech", "news" },
                    Interests = new List<string> { "AI", "programming" }
                },
                new User
                {
                    Username = "user2",
                    Email = "user2@example.com",
                    Preferences = new List<string> { "sports", "entertainment" },
                    Interests = new List<string> { "football", "movies" }
                }
            };
            
            context.Users.AddRange(users);
            context.SaveChanges();
            
            // Add sample content
            var contents = new List<Content>
            {
                new Content
                {
                    Title = "Introduction to Elasticsearch",
                    Description = "Learn the basics of Elasticsearch",
                    ContentType = "article",
                    Categories = new List<string> { "tech", "tutorial" },
                    Tags = new List<string> { "elasticsearch", "search", "database" }
                },
                new Content
                {
                    Title = "Machine Learning Trends",
                    Description = "Latest trends in ML",
                    ContentType = "article",
                    Categories = new List<string> { "tech", "ai" },
                    Tags = new List<string> { "machine learning", "ai", "trends" }
                }
            };
            
            context.Content.AddRange(contents);
            context.SaveChanges();
            
            logger.LogInformation("Added {UserCount} users and {ContentCount} content items", users.Count, contents.Count);
        }
    }
}
