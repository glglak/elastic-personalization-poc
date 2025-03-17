using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticPersonalization.Core.Entities;
using ElasticPersonalization.Core.Interfaces;
using ElasticPersonalization.Core.Models;
using ElasticPersonalization.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;

namespace ElasticPersonalization.Infrastructure.Services
{
    public class ContentService : IContentService
    {
        private readonly ContentActionsDbContext _dbContext;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ContentService> _logger;
        
        public ContentService(
            ContentActionsDbContext dbContext,
            IElasticClient elasticClient,
            ILogger<ContentService> logger)
        {
            _dbContext = dbContext;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<ContentDto> GetContentAsync(string contentId)
        {
            try
            {
                var content = await _dbContext.Content
                    .Include(c => c.Creator)
                    .Include(c => c.Likes)
                    .Include(c => c.Comments)
                    .Include(c => c.Shares)
                    .FirstOrDefaultAsync(c => c.Id == contentId);
                
                if (content == null)
                {
                    throw new ArgumentException($"Content with ID {contentId} not found");
                }
                
                return MapToContentDto(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content with ID {ContentId}", contentId);
                throw;
            }
        }

        public async Task<ContentDto> CreateContentAsync(CreateContentDto contentDto)
        {
            try
            {
                // Verify creator exists
                var creator = await _dbContext.Users.FindAsync(contentDto.CreatorId);
                if (creator == null)
                {
                    throw new ArgumentException($"Creator with ID {contentDto.CreatorId} not found");
                }
                
                // Create content entity
                var content = new Content
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = contentDto.Title,
                    Description = contentDto.Description,
                    Body = contentDto.Body,
                    CreatedAt = DateTime.UtcNow,
                    Tags = contentDto.Tags,
                    Categories = contentDto.Categories,
                    CreatorId = contentDto.CreatorId
                };
                
                // Save to database
                _dbContext.Content.Add(content);
                await _dbContext.SaveChangesAsync();
                
                // Index in Elasticsearch
                await SyncContentToElasticsearchAsync(content);
                
                return MapToContentDto(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating content: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ContentDto> UpdateContentAsync(string contentId, UpdateContentDto contentDto)
        {
            try
            {
                var content = await _dbContext.Content.FindAsync(contentId);
                if (content == null)
                {
                    throw new ArgumentException($"Content with ID {contentId} not found");
                }
                
                // Update only the provided fields
                if (contentDto.Title != null)
                    content.Title = contentDto.Title;
                
                if (contentDto.Description != null)
                    content.Description = contentDto.Description;
                
                if (contentDto.Body != null)
                    content.Body = contentDto.Body;
                
                if (contentDto.Tags != null)
                    content.Tags = contentDto.Tags;
                
                if (contentDto.Categories != null)
                    content.Categories = contentDto.Categories;
                
                // Save to database
                _dbContext.Content.Update(content);
                await _dbContext.SaveChangesAsync();
                
                // Update in Elasticsearch
                await SyncContentToElasticsearchAsync(content);
                
                return MapToContentDto(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating content with ID {ContentId}", contentId);
                throw;
            }
        }

        public async Task DeleteContentAsync(string contentId)
        {
            try
            {
                var content = await _dbContext.Content.FindAsync(contentId);
                if (content == null)
                {
                    throw new ArgumentException($"Content with ID {contentId} not found");
                }
                
                // Remove from database
                _dbContext.Content.Remove(content);
                await _dbContext.SaveChangesAsync();
                
                // Remove from Elasticsearch
                await RemoveContentFromElasticsearchAsync(contentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting content with ID {ContentId}", contentId);
                throw;
            }
        }

        public async Task<List<ContentDto>> SearchContentAsync(string query, int page = 1, int pageSize = 20)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<Content>(s => s
                    .From((page - 1) * pageSize)
                    .Size(pageSize)
                    .Query(q => q
                        .MultiMatch(m => m
                            .Fields(f => f
                                .Field(p => p.Title, 2.0)
                                .Field(p => p.Description, 1.5)
                                .Field(p => p.Body)
                                .Field(p => p.Tags, 1.5)
                                .Field(p => p.Categories, 1.5)
                            )
                            .Query(query)
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(Fuzziness.Auto)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Error searching for content: {Error}", searchResponse.DebugInformation);
                    throw new Exception("Error searching for content: " + searchResponse.DebugInformation);
                }

                if (searchResponse.Total == 0)
                {
                    return new List<ContentDto>();
                }

                var contentIds = searchResponse.Documents.Select(c => c.Id).ToList();
                var contentItems = await _dbContext.Content
                    .Include(c => c.Creator)
                    .Include(c => c.Likes)
                    .Include(c => c.Comments)
                    .Include(c => c.Shares)
                    .Where(c => contentIds.Contains(c.Id))
                    .ToListAsync();

                // Preserve order from search results
                var orderedContent = contentIds
                    .Select(id => contentItems.FirstOrDefault(c => c.Id == id))
                    .Where(c => c != null)
                    .ToList();

                return orderedContent.Select(MapToContentDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for content with query: {Query}", query);
                throw;
            }
        }

        public async Task<List<ContentDto>> GetContentByCategoryAsync(string category, int page = 1, int pageSize = 20)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<Content>(s => s
                    .From((page - 1) * pageSize)
                    .Size(pageSize)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Categories)
                            .Query(category)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Error searching for content by category: {Error}", searchResponse.DebugInformation);
                    throw new Exception("Error searching for content by category: " + searchResponse.DebugInformation);
                }

                var contentIds = searchResponse.Documents.Select(c => c.Id).ToList();
                var contentItems = await _dbContext.Content
                    .Include(c => c.Creator)
                    .Include(c => c.Likes)
                    .Include(c => c.Comments)
                    .Include(c => c.Shares)
                    .Where(c => contentIds.Contains(c.Id))
                    .ToListAsync();

                var orderedContent = contentIds
                    .Select(id => contentItems.FirstOrDefault(c => c.Id == id))
                    .Where(c => c != null)
                    .ToList();

                return orderedContent.Select(MapToContentDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content by category: {Category}", category);
                throw;
            }
        }

        public async Task<List<ContentDto>> GetContentByTagAsync(string tag, int page = 1, int pageSize = 20)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<Content>(s => s
                    .From((page - 1) * pageSize)
                    .Size(pageSize)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Tags)
                            .Query(tag)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Error searching for content by tag: {Error}", searchResponse.DebugInformation);
                    throw new Exception("Error searching for content by tag: " + searchResponse.DebugInformation);
                }

                var contentIds = searchResponse.Documents.Select(c => c.Id).ToList();
                var contentItems = await _dbContext.Content
                    .Include(c => c.Creator)
                    .Include(c => c.Likes)
                    .Include(c => c.Comments)
                    .Include(c => c.Shares)
                    .Where(c => contentIds.Contains(c.Id))
                    .ToListAsync();

                var orderedContent = contentIds
                    .Select(id => contentItems.FirstOrDefault(c => c.Id == id))
                    .Where(c => c != null)
                    .ToList();

                return orderedContent.Select(MapToContentDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content by tag: {Tag}", tag);
                throw;
            }
        }

        public async Task<List<ContentDto>> GetContentByCreatorAsync(int creatorId, int page = 1, int pageSize = 20)
        {
            try
            {
                var contentItems = await _dbContext.Content
                    .Include(c => c.Creator)
                    .Include(c => c.Likes)
                    .Include(c => c.Comments)
                    .Include(c => c.Shares)
                    .Where(c => c.CreatorId == creatorId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return contentItems.Select(MapToContentDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content by creator: {CreatorId}", creatorId);
                throw;
            }
        }

        public async Task SyncContentToElasticsearchAsync(Content content)
        {
            try
            {
                var response = await _elasticClient.IndexDocumentAsync(content);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Failed to index content in Elasticsearch: {Error}", response.DebugInformation);
                    throw new Exception("Failed to index content in Elasticsearch: " + response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing content to Elasticsearch: {ContentId}", content.Id);
                throw;
            }
        }

        public async Task RemoveContentFromElasticsearchAsync(string contentId)
        {
            try
            {
                var response = await _elasticClient.DeleteAsync<Content>(contentId);
                
                if (!response.IsValid && response.Result != Result.NotFound)
                {
                    _logger.LogError("Failed to remove content from Elasticsearch: {Error}", response.DebugInformation);
                    throw new Exception("Failed to remove content from Elasticsearch: " + response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing content from Elasticsearch: {ContentId}", contentId);
                throw;
            }
        }

        // Helper method to map Content entity to ContentDto
        private ContentDto MapToContentDto(Content content)
        {
            return new ContentDto
            {
                Id = content.Id,
                Title = content.Title,
                Description = content.Description,
                Body = content.Body,
                CreatedAt = content.CreatedAt,
                Tags = content.Tags,
                Categories = content.Categories,
                CreatorId = content.CreatorId,
                CreatorUsername = content.Creator?.Username ?? "Unknown",
                ShareCount = content.Shares?.Count ?? 0,
                LikeCount = content.Likes?.Count ?? 0,
                CommentCount = content.Comments?.Count ?? 0
            };
        }
    }
}
