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

        public async Task<ContentDto> GetContentAsync(int contentId)
        {
            try
            {
                var content = await _dbContext.Content
                    .Include(c => c.Creator)
                    .FirstOrDefaultAsync(c => c.Id == contentId);

                if (content == null)
                {
                    return null;
                }

                // Get interaction counts
                var shareCount = await _dbContext.Shares.CountAsync(s => s.ContentId == contentId);
                var likeCount = await _dbContext.Likes.CountAsync(l => l.ContentId == contentId);
                var commentCount = await _dbContext.Comments.CountAsync(c => c.ContentId == contentId);

                return MapToContentDto(content, shareCount, likeCount, commentCount);
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
                // Check if creator exists
                var creator = await _dbContext.Users.FindAsync(contentDto.CreatorId);
                if (creator == null)
                {
                    throw new ArgumentException($"Creator with ID {contentDto.CreatorId} not found");
                }

                // Create content entity
                var content = new Content
                {
                    Title = contentDto.Title,
                    Description = contentDto.Description,
                    Body = contentDto.Body,
                    ContentText = contentDto.Body, // Keep the ContentText property in sync
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatorId = contentDto.CreatorId,
                    Tags = contentDto.Tags,
                    Categories = contentDto.Categories
                };

                _dbContext.Content.Add(content);
                await _dbContext.SaveChangesAsync();

                // Index in Elasticsearch
                await SyncContentToElasticsearchAsync(content);

                return MapToContentDto(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating content");
                throw;
            }
        }

        public async Task<ContentDto> UpdateContentAsync(int contentId, UpdateContentDto contentDto)
        {
            try
            {
                var content = await _dbContext.Content.FindAsync(contentId);
                if (content == null)
                {
                    return null;
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(contentDto.Title))
                {
                    content.Title = contentDto.Title;
                }

                if (!string.IsNullOrEmpty(contentDto.Description))
                {
                    content.Description = contentDto.Description;
                }

                if (!string.IsNullOrEmpty(contentDto.Body))
                {
                    content.Body = contentDto.Body;
                    content.ContentText = contentDto.Body; // Keep the ContentText property in sync
                }

                if (contentDto.Tags != null)
                {
                    content.Tags = contentDto.Tags;
                }

                if (contentDto.Categories != null)
                {
                    content.Categories = contentDto.Categories;
                }

                content.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                // Update in Elasticsearch
                await SyncContentToElasticsearchAsync(content);

                return await GetContentAsync(contentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating content with ID {ContentId}", contentId);
                throw;
            }
        }

        public async Task DeleteContentAsync(int contentId)
        {
            try
            {
                var content = await _dbContext.Content.FindAsync(contentId);
                if (content == null)
                {
                    return;
                }

                // First delete related interactions
                var shares = await _dbContext.Shares.Where(s => s.ContentId == contentId).ToListAsync();
                var likes = await _dbContext.Likes.Where(l => l.ContentId == contentId).ToListAsync();
                var comments = await _dbContext.Comments.Where(c => c.ContentId == contentId).ToListAsync();

                _dbContext.Shares.RemoveRange(shares);
                _dbContext.Likes.RemoveRange(likes);
                _dbContext.Comments.RemoveRange(comments);

                // Then delete the content
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
                    .Index(_elasticClient.ConnectionSettings.DefaultIndex)
                    .From((page - 1) * pageSize)
                    .Size(pageSize)
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Fields(f => f
                                .Field(c => c.Title, 2.0)
                                .Field(c => c.Description, 1.5)
                                .Field(c => c.Body)
                                .Field(c => c.Tags, 1.5)
                                .Field(c => c.Categories, 1.5)
                            )
                            .Query(query)
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(Fuzziness.Auto)
                        )
                    )
                    .Sort(sort => sort
                        .Descending("_score")
                        .Descending(c => c.CreatedAt)
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Elasticsearch search failed: {Error}", searchResponse.DebugInformation);
                    return new List<ContentDto>();
                }

                return await MapSearchResultsToContentDtosAsync(searchResponse.Documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching content with query: {Query}", query);
                throw;
            }
        }

        public async Task<List<ContentDto>> GetContentByCategoryAsync(string category, int page = 1, int pageSize = 20)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<Content>(s => s
                    .Index(_elasticClient.ConnectionSettings.DefaultIndex)
                    .From((page - 1) * pageSize)
                    .Size(pageSize)
                    .Query(q => q
                        .Match(m => m
                            .Field(c => c.Categories)
                            .Query(category)
                        )
                    )
                    .Sort(sort => sort
                        .Descending(c => c.CreatedAt)
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Elasticsearch category search failed: {Error}", searchResponse.DebugInformation);
                    return new List<ContentDto>();
                }

                return await MapSearchResultsToContentDtosAsync(searchResponse.Documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content by category: {Category}", category);
                throw;
            }
        }

        public async Task<List<ContentDto>> GetContentByTagAsync(string tag, int page = 1, int pageSize = 20)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<Content>(s => s
                    .Index(_elasticClient.ConnectionSettings.DefaultIndex)
                    .From((page - 1) * pageSize)
                    .Size(pageSize)
                    .Query(q => q
                        .Match(m => m
                            .Field(c => c.Tags)
                            .Query(tag)
                        )
                    )
                    .Sort(sort => sort
                        .Descending(c => c.CreatedAt)
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Elasticsearch tag search failed: {Error}", searchResponse.DebugInformation);
                    return new List<ContentDto>();
                }

                return await MapSearchResultsToContentDtosAsync(searchResponse.Documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content by tag: {Tag}", tag);
                throw;
            }
        }

        public async Task<List<ContentDto>> GetContentByCreatorAsync(int creatorId, int page = 1, int pageSize = 20)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<Content>(s => s
                    .Index(_elasticClient.ConnectionSettings.DefaultIndex)
                    .From((page - 1) * pageSize)
                    .Size(pageSize)
                    .Query(q => q
                        .Term(t => t
                            .Field(c => c.CreatorId)
                            .Value(creatorId)
                        )
                    )
                    .Sort(sort => sort
                        .Descending(c => c.CreatedAt)
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Elasticsearch creator search failed: {Error}", searchResponse.DebugInformation);
                    return new List<ContentDto>();
                }

                return await MapSearchResultsToContentDtosAsync(searchResponse.Documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content by creator: {CreatorId}", creatorId);
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing content to Elasticsearch: {ContentId}", content.Id);
                throw;
            }
        }

        public async Task RemoveContentFromElasticsearchAsync(int contentId)
        {
            try
            {
                var response = await _elasticClient.DeleteAsync<Content>(contentId.ToString());
                
                if (!response.IsValid && response.Result != Result.NotFound)
                {
                    _logger.LogError("Failed to remove content from Elasticsearch: {Error}", response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing content from Elasticsearch: {ContentId}", contentId);
                throw;
            }
        }

        // Helper methods
        private ContentDto MapToContentDto(Content content, int? shareCount = null, int? likeCount = null, int? commentCount = null)
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
                CreatorUsername = content.Creator?.Username ?? "",
                ShareCount = shareCount ?? 0,
                LikeCount = likeCount ?? 0,
                CommentCount = commentCount ?? 0
            };
        }

        private async Task<List<ContentDto>> MapSearchResultsToContentDtosAsync(IEnumerable<Content> contents)
        {
            var contentIds = contents.Select(c => c.Id).ToList();
            
            // Get all interaction counts in one go
            var shareCounts = await _dbContext.Shares
                .Where(s => contentIds.Contains(s.ContentId))
                .GroupBy(s => s.ContentId)
                .Select(g => new { ContentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ContentId, x => x.Count);
                
            var likeCounts = await _dbContext.Likes
                .Where(l => contentIds.Contains(l.ContentId))
                .GroupBy(l => l.ContentId)
                .Select(g => new { ContentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ContentId, x => x.Count);
                
            var commentCounts = await _dbContext.Comments
                .Where(c => contentIds.Contains(c.ContentId))
                .GroupBy(c => c.ContentId)
                .Select(g => new { ContentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ContentId, x => x.Count);
                
            // Get creator usernames
            var creatorIds = contents.Select(c => c.CreatorId).Distinct().ToList();
            var creators = await _dbContext.Users
                .Where(u => creatorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);
                
            // Map to DTOs
            return contents.Select(content => new ContentDto
            {
                Id = content.Id,
                Title = content.Title,
                Description = content.Description,
                Body = content.Body,
                CreatedAt = content.CreatedAt,
                Tags = content.Tags,
                Categories = content.Categories,
                CreatorId = content.CreatorId,
                CreatorUsername = creators.GetValueOrDefault(content.CreatorId, ""),
                ShareCount = shareCounts.GetValueOrDefault(content.Id, 0),
                LikeCount = likeCounts.GetValueOrDefault(content.Id, 0),
                CommentCount = commentCounts.GetValueOrDefault(content.Id, 0)
            }).ToList();
        }
    }
}