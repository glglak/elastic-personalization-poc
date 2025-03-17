using ElasticPersonalization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticPersonalization.Infrastructure.Data
{
    public class SeedData
    {
        private readonly ContentActionsDbContext _dbContext;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<SeedData> _logger;

        public SeedData(
            ContentActionsDbContext dbContext,
            IElasticClient elasticClient,
            ILogger<SeedData> logger)
        {
            _dbContext = dbContext;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task SeedAllAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding process...");
                
                await SeedUsersAsync();
                await SeedContentAsync();
                await SeedInteractionsAsync();
                
                await IndexContentInElasticsearchAsync();
                
                _logger.LogInformation("Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data seeding process");
                throw;
            }
        }

        private async Task SeedUsersAsync()
        {
            if (!await _dbContext.Users.AnyAsync())
            {
                _logger.LogInformation("Seeding users data...");
                
                var users = new[]
                {
                    new User { Username = "user1", Email = "user1@example.com" },
                    new User { Username = "user2", Email = "user2@example.com" },
                    new User { Username = "user3", Email = "user3@example.com" },
                    new User { Username = "user4", Email = "user4@example.com" },
                    new User { Username = "user5", Email = "user5@example.com" }
                };
                
                await _dbContext.Users.AddRangeAsync(users);
                await _dbContext.SaveChangesAsync();
                
                // Add preferences and interests
                var userPreferences = new List<(int UserId, string Preference)>
                {
                    (1, "Programming"),
                    (1, "Database"),
                    (2, "DevOps"),
                    (2, "Web Development"),
                    (3, "Database"),
                    (3, "Performance"),
                    (4, "Search"),
                    (4, "User Experience"),
                    (5, "DevOps"),
                    (5, "Containers")
                };
                
                var userInterests = new List<(int UserId, string Interest)>
                {
                    (1, "elasticsearch"),
                    (1, "dotnet"),
                    (2, "docker"),
                    (2, "programming"),
                    (3, "sql"),
                    (3, "database"),
                    (4, "search"),
                    (4, "user-experience"),
                    (5, "containerization"),
                    (5, "devops")
                };
                
                foreach (var (userId, preference) in userPreferences)
                {
                    var user = await _dbContext.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.Preferences.Add(preference);
                    }
                }
                
                foreach (var (userId, interest) in userInterests)
                {
                    var user = await _dbContext.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.Interests.Add(interest);
                    }
                }
                
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation("Users already exist, skipping seeding");
            }
        }

        private async Task SeedContentAsync()
        {
            if (!await _dbContext.Content.AnyAsync())
            {
                _logger.LogInformation("Seeding content data...");
                
                var contents = new[]
                {
                    new Content
                    {
                        Id = "1",
                        Title = "Introduction to Elasticsearch",
                        Description = "Learn the basics of Elasticsearch",
                        Body = "Elasticsearch is a distributed, RESTful search and analytics engine capable of addressing a growing number of use cases. As the heart of the Elastic Stack, it centrally stores your data for lightning-fast search, relevance, and powerful analytics.",
                        CreatedAt = DateTime.UtcNow,
                        CreatorId = 1,
                        Tags = new List<string> { "elasticsearch", "search", "database" },
                        Categories = new List<string> { "Database", "Search" }
                    },
                    new Content
                    {
                        Id = "2",
                        Title = "Advanced .NET Core Development",
                        Description = "Take your .NET skills to the next level",
                        Body = "In this article, we explore advanced concepts in .NET Core development including dependency injection, middleware, and microservices architecture. We'll also cover best practices for performance optimization and application security.",
                        CreatedAt = DateTime.UtcNow,
                        CreatorId = 2,
                        Tags = new List<string> { "dotnet", "csharp", "programming" },
                        Categories = new List<string> { "Programming", "Web Development" }
                    },
                    new Content
                    {
                        Id = "3",
                        Title = "Building Personalized Content Feeds",
                        Description = "Learn how to create personalized experiences",
                        Body = "Personalization is key to user engagement. This article explains how to build personalized content feeds using a combination of user preferences, interests, and interaction history. We'll demonstrate practical examples using Elasticsearch and SQL Server.",
                        CreatedAt = DateTime.UtcNow,
                        CreatorId = 1,
                        Tags = new List<string> { "personalization", "recommendation", "user-experience" },
                        Categories = new List<string> { "User Experience", "Personalization" }
                    },
                    new Content
                    {
                        Id = "4",
                        Title = "SQL Server Performance Tuning",
                        Description = "Optimize your database queries",
                        Body = "This guide covers essential techniques for SQL Server optimization including index management, query analysis, and execution plans. Learn how to improve database performance and reduce resource usage through practical examples and case studies.",
                        CreatedAt = DateTime.UtcNow,
                        CreatorId = 3,
                        Tags = new List<string> { "sql", "database", "performance" },
                        Categories = new List<string> { "Database", "Performance" }
                    },
                    new Content
                    {
                        Id = "5",
                        Title = "Getting Started with Docker",
                        Description = "Containerize your applications",
                        Body = "Docker is a platform for developing, shipping, and running applications in containers. This beginner-friendly guide will take you through the basics of Docker, including containers, images, and Docker Compose for multi-container applications.",
                        CreatedAt = DateTime.UtcNow,
                        CreatorId = 2,
                        Tags = new List<string> { "docker", "containerization", "devops" },
                        Categories = new List<string> { "DevOps", "Containers" }
                    }
                };
                
                await _dbContext.Content.AddRangeAsync(contents);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation("Content already exists, skipping seeding");
            }
        }

        private async Task SeedInteractionsAsync()
        {
            if (!await _dbContext.Likes.AnyAsync() && !await _dbContext.Comments.AnyAsync() && !await _dbContext.Shares.AnyAsync() && !await _dbContext.Follows.AnyAsync())
            {
                _logger.LogInformation("Seeding user interactions...");
                
                // Sample follows
                var follows = new[]
                {
                    new UserFollow { UserId = 1, FollowedUserId = 2, CreatedAt = DateTime.UtcNow },
                    new UserFollow { UserId = 1, FollowedUserId = 3, CreatedAt = DateTime.UtcNow },
                    new UserFollow { UserId = 2, FollowedUserId = 1, CreatedAt = DateTime.UtcNow },
                    new UserFollow { UserId = 3, FollowedUserId = 1, CreatedAt = DateTime.UtcNow },
                    new UserFollow { UserId = 4, FollowedUserId = 1, CreatedAt = DateTime.UtcNow },
                    new UserFollow { UserId = 4, FollowedUserId = 2, CreatedAt = DateTime.UtcNow },
                    new UserFollow { UserId = 5, FollowedUserId = 2, CreatedAt = DateTime.UtcNow }
                };
                
                // Sample likes
                var likes = new[]
                {
                    new UserLike { UserId = 1, ContentId = "2", CreatedAt = DateTime.UtcNow },
                    new UserLike { UserId = 1, ContentId = "4", CreatedAt = DateTime.UtcNow },
                    new UserLike { UserId = 2, ContentId = "1", CreatedAt = DateTime.UtcNow },
                    new UserLike { UserId = 2, ContentId = "3", CreatedAt = DateTime.UtcNow },
                    new UserLike { UserId = 3, ContentId = "1", CreatedAt = DateTime.UtcNow },
                    new UserLike { UserId = 3, ContentId = "5", CreatedAt = DateTime.UtcNow },
                    new UserLike { UserId = 4, ContentId = "3", CreatedAt = DateTime.UtcNow },
                    new UserLike { UserId = 5, ContentId = "2", CreatedAt = DateTime.UtcNow },
                    new UserLike { UserId = 5, ContentId = "5", CreatedAt = DateTime.UtcNow }
                };
                
                // Sample comments
                var comments = new[]
                {
                    new UserComment { UserId = 1, ContentId = "2", CommentText = "Great article on .NET Core!", CreatedAt = DateTime.UtcNow },
                    new UserComment { UserId = 2, ContentId = "1", CommentText = "Very helpful introduction to Elasticsearch", CreatedAt = DateTime.UtcNow },
                    new UserComment { UserId = 3, ContentId = "4", CommentText = "These optimization tips worked well for me", CreatedAt = DateTime.UtcNow },
                    new UserComment { UserId = 4, ContentId = "3", CommentText = "Love the personalization insights", CreatedAt = DateTime.UtcNow },
                    new UserComment { UserId = 5, ContentId = "5", CommentText = "Docker is changing how we deploy applications", CreatedAt = DateTime.UtcNow }
                };
                
                // Sample shares
                var shares = new[]
                {
                    new UserShare { UserId = 1, ContentId = "3", CreatedAt = DateTime.UtcNow },
                    new UserShare { UserId = 2, ContentId = "2", CreatedAt = DateTime.UtcNow },
                    new UserShare { UserId = 3, ContentId = "1", CreatedAt = DateTime.UtcNow },
                    new UserShare { UserId = 4, ContentId = "5", CreatedAt = DateTime.UtcNow },
                    new UserShare { UserId = 5, ContentId = "4", CreatedAt = DateTime.UtcNow }
                };
                
                await _dbContext.Follows.AddRangeAsync(follows);
                await _dbContext.Likes.AddRangeAsync(likes);
                await _dbContext.Comments.AddRangeAsync(comments);
                await _dbContext.Shares.AddRangeAsync(shares);
                
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation("Interactions already exist, skipping seeding");
            }
        }

        private async Task IndexContentInElasticsearchAsync()
        {
            try
            {
                _logger.LogInformation("Indexing content in Elasticsearch...");
                
                // Check if the index exists
                var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(_elasticClient.ConnectionSettings.DefaultIndex);
                
                // If the index doesn't exist, create it with mappings
                if (!indexExistsResponse.Exists)
                {
                    var createIndexResponse = await _elasticClient.Indices.CreateAsync(_elasticClient.ConnectionSettings.DefaultIndex, c => c
                        .Settings(s => s
                            .NumberOfShards(1)
                            .NumberOfReplicas(0)
                            .Analysis(a => a
                                .Analyzers(an => an
                                    .Custom("content_analyzer", ca => ca
                                        .Tokenizer("standard")
                                        .Filters("lowercase", "asciifolding", "stop", "snowball")
                                    )
                                )
                            )
                        )
                        .Map<Content>(m => m
                            .Properties(p => p
                                .Keyword(k => k.Name(c => c.Id))
                                .Text(t => t
                                    .Name(c => c.Title)
                                    .Analyzer("content_analyzer")
                                    .Boost(2.0)
                                    .Fields(f => f
                                        .Keyword(k => k.Name("keyword"))
                                    )
                                )
                                .Text(t => t
                                    .Name(c => c.Description)
                                    .Analyzer("content_analyzer")
                                    .Boost(1.5)
                                )
                                .Text(t => t
                                    .Name(c => c.Body)
                                    .Analyzer("content_analyzer")
                                )
                                .Date(d => d.Name(c => c.CreatedAt))
                                .Keyword(k => k
                                    .Name(c => c.Tags)
                                    .Boost(1.5)
                                )
                                .Keyword(k => k
                                    .Name(c => c.Categories)
                                    .Boost(1.5)
                                )
                                .Number(n => n.Name(c => c.CreatorId).Type(NumberType.Integer))
                            )
                        )
                    );
                    
                    if (!createIndexResponse.IsValid)
                    {
                        _logger.LogError("Failed to create Elasticsearch index: {Error}", createIndexResponse.DebugInformation);
                        return;
                    }
                }
                
                // Get content to index
                var contentToIndex = await _dbContext.Content
                    .Include(c => c.Creator)
                    .ToListAsync();
                
                if (!contentToIndex.Any())
                {
                    _logger.LogInformation("No content to index");
                    return;
                }
                
                // Create bulk descriptor
                var bulkDescriptor = new BulkDescriptor();
                
                foreach (var content in contentToIndex)
                {
                    bulkDescriptor.Index<Content>(i => i
                        .Index(_elasticClient.ConnectionSettings.DefaultIndex)
                        .Id(content.Id)
                        .Document(content)
                    );
                }
                
                // Execute the bulk index
                var bulkResponse = await _elasticClient.BulkAsync(bulkDescriptor);
                
                if (!bulkResponse.IsValid)
                {
                    _logger.LogError("Failed to index content in Elasticsearch: {Error}", bulkResponse.DebugInformation);
                }
                else
                {
                    _logger.LogInformation("Successfully indexed {Count} documents in Elasticsearch", contentToIndex.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing content in Elasticsearch");
                throw;
            }
        }
    }
}
