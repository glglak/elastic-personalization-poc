using System;
using System.Collections.Generic;
using System.Linq;
using ElasticPersonalization.Core.Entities;
using ElasticPersonalization.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElasticPersonalization.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                logger.LogInformation("Starting database initialization...");
                
                // Get the database context
                using var dbContext = serviceProvider.GetRequiredService<ContentActionsDbContext>();
                
                // Check if the database exists, and create it if it doesn't
                logger.LogInformation("Ensuring database exists...");
                dbContext.Database.EnsureCreated();
                
                // Add sample data if the tables are empty
                AddSampleData(dbContext, logger);
                
                logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw; // Re-throw to ensure application fails on startup if DB init fails
            }
        }
        
        private static void AddSampleData(ContentActionsDbContext dbContext, ILogger logger)
        {
            // Check if there's already data
            if (dbContext.Users.Any() && dbContext.Content.Any())
            {
                logger.LogInformation("Database already contains data, skipping sample data creation.");
                return;
            }
            
            logger.LogInformation("Adding sample data...");
            
            // Add users if they don't exist
            if (!dbContext.Users.Any())
            {
                logger.LogInformation("Adding sample users...");
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
                
                dbContext.Users.AddRange(users);
                dbContext.SaveChanges();
                logger.LogInformation("Added {Count} sample users.", users.Count);
            }
            
            // Add content if it doesn't exist
            if (!dbContext.Content.Any())
            {
                logger.LogInformation("Adding sample content...");
                var contentItems = new List<Content>
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
                
                dbContext.Content.AddRange(contentItems);
                dbContext.SaveChanges();
                logger.LogInformation("Added {Count} sample content items.", contentItems.Count);
            }
            
            // Add interactions if they don't exist
            if (!dbContext.Likes.Any())
            {
                logger.LogInformation("Adding sample interactions...");
                
                var users = dbContext.Users.ToList();
                var contentItems = dbContext.Content.ToList();
                
                if (users.Count >= 2 && contentItems.Count >= 2)
                {
                    // Add likes
                    var likes = new List<UserLike>
                    {
                        new UserLike { UserId = users[0].Id, ContentId = contentItems[0].Id },
                        new UserLike { UserId = users[1].Id, ContentId = contentItems[1].Id }
                    };
                    dbContext.Likes.AddRange(likes);
                    
                    // Add comments
                    var comments = new List<UserComment>
                    {
                        new UserComment { UserId = users[0].Id, ContentId = contentItems[0].Id, CommentText = "Great introduction!" },
                        new UserComment { UserId = users[1].Id, ContentId = contentItems[1].Id, CommentText = "Very informative content." }
                    };
                    dbContext.Comments.AddRange(comments);
                    
                    // Add follows
                    var follows = new List<UserFollow>
                    {
                        new UserFollow { UserId = users[0].Id, FollowedUserId = users[1].Id },
                    };
                    dbContext.Follows.AddRange(follows);
                    
                    dbContext.SaveChanges();
                    logger.LogInformation("Added sample interactions.");
                }
            }
        }
    }
}