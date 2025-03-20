using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticPersonalization.Core.Configuration;
using ElasticPersonalization.Core.Entities;
using ElasticPersonalization.Core.Interfaces;
using ElasticPersonalization.Core.Models;
using ElasticPersonalization.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace ElasticPersonalization.Infrastructure.Services
{
    public class PersonalizationService : IPersonalizationService
    {
        private readonly ContentActionsDbContext _dbContext;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<PersonalizationService> _logger;
        private readonly PersonalizationWeights _weights;

        public PersonalizationService(
            ContentActionsDbContext dbContext,
            IElasticClient elasticClient,
            IOptions<PersonalizationWeights> weightsOptions,
            ILogger<PersonalizationService> logger)
        {
            _dbContext = dbContext;
            _elasticClient = elasticClient;
            _logger = logger;
            _weights = weightsOptions.Value;
        }

        public async Task<List<ContentDto>> GetPersonalizedFeedAsync(int userId, int page = 1, int pageSize = 20)
        {
            try
            {
                // Get user data including interactions - remove invalid includes for Preferences and Interests
                var user = await _dbContext.Users
                    .Include(u => u.Shares)
                    .Include(u => u.Likes)
                    .Include(u => u.Comments)
                    .Include(u => u.Following)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                // Get personalization factors for search query composition
                var personalizationFactors = await GetPersonalizationFactorsAsync(userId);

                // Extract user preferences and interests for content matching
                var userPreferences = user.Preferences ?? new List<string>();
                var userInterests = user.Interests ?? new List<string>();
                
                // Get users that this user follows
                var followedUserIds = user.Following.Select(f => f.FollowedUserId).ToList();

                // Combine all these signals into an Elasticsearch query
                var searchResponse = await _elasticClient.SearchAsync<Content>(s => s
                    .From((page - 1) * pageSize)
                    .Size(pageSize)
                    .Query(q => q
                        .FunctionScore(fs => fs
                            .Query(fq => BuildBaseQuery(userPreferences, userInterests, followedUserIds))
                            .Functions(BuildScoringFunctions(personalizationFactors))
                            .ScoreMode(FunctionScoreMode.Sum)
                            .BoostMode(FunctionBoostMode.Multiply)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Error searching for personalized content: {Error}", searchResponse.DebugInformation);
                    throw new Exception("Error searching for personalized content: " + searchResponse.DebugInformation);
                }

                var contentIds = searchResponse.Documents.Select(c => c.Id).ToList();
                
                // Fetch full content with related data from database
                var contentItems = await _dbContext.Content
                    .Include(c => c.Creator)
                    .Include(c => c.Likes)
                    .Include(c => c.Comments)
                    .Include(c => c.Shares)
                    .Where(c => contentIds.Contains(c.Id))
                    .ToListAsync();

                // Preserve the order from search results
                var orderedContent = contentIds
                    .Select(id => contentItems.FirstOrDefault(c => c.Id == id))
                    .Where(c => c != null)
                    .ToList();

                // Map to DTOs with personalization scores
                var contentDtos = new List<ContentDto>();
                foreach (var content in orderedContent)
                {
                    if (content != null)
                    {
                        var dto = MapToContentDto(content);
                        
                        // Calculate personalization score for transparency
                        dto.PersonalizationScore = await CalculatePersonalizationScoreAsync(userId, content.Id);
                        
                        contentDtos.Add(dto);
                    }
                }

                return contentDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating personalized feed for user {UserId}", userId);
                throw;
            }
        }

        public async Task<double> CalculatePersonalizationScoreAsync(int userId, string contentId)
        {
            try
            {
                // Get user data - remove invalid includes for Preferences and Interests
                var user = await _dbContext.Users
                    .Include(u => u.Shares.Where(s => s.ContentId == contentId))
                    .Include(u => u.Likes.Where(l => l.ContentId == contentId))
                    .Include(u => u.Comments.Where(c => c.ContentId == contentId))
                    .Include(u => u.Following)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                // Get content
                var content = await _dbContext.Content
                    .Include(c => c.Creator)
                    .Include(c => c.Tags)
                    .Include(c => c.Categories)
                    .FirstOrDefaultAsync(c => c.Id == contentId);

                if (content == null)
                {
                    throw new ArgumentException($"Content with ID {contentId} not found");
                }

                double score = 1.0; // Base score

                // Direct interactions with this content
                if (user.Shares.Any(s => s.ContentId == contentId))
                {
                    score += _weights.ShareWeight;
                }

                if (user.Likes.Any(l => l.ContentId == contentId))
                {
                    score += _weights.LikeWeight;
                }

                if (user.Comments.Any(c => c.ContentId == contentId))
                {
                    score += _weights.CommentWeight;
                }

                // Creator relationship (followed by user)
                if (user.Following.Any(f => f.FollowedUserId == content.CreatorId))
                {
                    score += _weights.FollowWeight;
                }

                // Preferences match
                var userPreferences = user.Preferences ?? new List<string>();
                var matchingPreferences = content.Categories.Count(c => userPreferences.Contains(c));
                if (matchingPreferences > 0)
                {
                    score += _weights.PreferenceWeight * (matchingPreferences / (double)Math.Max(1, content.Categories.Count));
                }

                // Interests match
                var userInterests = user.Interests ?? new List<string>();
                var matchingInterests = content.Tags.Count(t => userInterests.Contains(t));
                if (matchingInterests > 0)
                {
                    score += _weights.InterestWeight * (matchingInterests / (double)Math.Max(1, content.Tags.Count));
                }

                return score;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating personalization score for user {UserId} and content {ContentId}", userId, contentId);
                throw;
            }
        }

        public async Task<PersonalizationFactors> GetPersonalizationFactorsAsync(int userId)
        {
            try
            {
                // Get user with all relevant data - remove invalid includes for Preferences and Interests
                var user = await _dbContext.Users
                    .Include(u => u.Shares)
                    .Include(u => u.Likes)
                    .Include(u => u.Comments)
                    .Include(u => u.Following)
                        .ThenInclude(f => f.FollowedUser)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                // Get content that the user has interacted with
                var interactedContentIds = new HashSet<string>(
                    user.Shares.Select(s => s.ContentId)
                    .Concat(user.Likes.Select(l => l.ContentId))
                    .Concat(user.Comments.Select(c => c.ContentId))
                );

                var interactedContent = await _dbContext.Content
                    .Where(c => interactedContentIds.Contains(c.Id))
                    .ToListAsync();

                // Calculate factors based on user interactions and weights
                var shareFactor = user.Shares.Count > 0 ? _weights.ShareWeight * user.Shares.Count : 0;
                var likeFactor = user.Likes.Count > 0 ? _weights.LikeWeight * user.Likes.Count : 0;
                var commentFactor = user.Comments.Count > 0 ? _weights.CommentWeight * user.Comments.Count : 0;
                var followFactor = user.Following.Count > 0 ? _weights.FollowWeight * user.Following.Count : 0;
                var preferenceFactor = user.Preferences.Count > 0 ? _weights.PreferenceWeight * user.Preferences.Count : 0;
                var interestFactor = user.Interests.Count > 0 ? _weights.InterestWeight * user.Interests.Count : 0;

                // Get most influential followed users (based on their activity)
                var influentialFollows = user.Following
                    .OrderByDescending(f => 
                        (_dbContext.Shares.Count(s => s.UserId == f.FollowedUserId) * _weights.ShareWeight) +
                        (_dbContext.Comments.Count(c => c.UserId == f.FollowedUserId) * _weights.CommentWeight) +
                        (_dbContext.Likes.Count(l => l.UserId == f.FollowedUserId) * _weights.LikeWeight)
                    )
                    .Take(5)
                    .Select(f => new UserFollowInfo
                    {
                        UserId = f.FollowedUserId,
                        Username = f.FollowedUser.Username,
                        InfluenceScore = _weights.FollowWeight + (
                            (_dbContext.Shares.Count(s => s.UserId == f.FollowedUserId) * _weights.ShareWeight) +
                            (_dbContext.Comments.Count(c => c.UserId == f.FollowedUserId) * _weights.CommentWeight) +
                            (_dbContext.Likes.Count(l => l.UserId == f.FollowedUserId) * _weights.LikeWeight)
                        ) / 10 // Normalize the score
                    })
                    .ToList();

                // Get recent significant interactions
                var recentInteractions = new List<ContentInteractionInfo>();
                
                // Add recent shares
                recentInteractions.AddRange(user.Shares
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(3)
                    .Select(s => new ContentInteractionInfo
                    {
                        ContentId = s.ContentId,
                        ContentTitle = interactedContent.FirstOrDefault(c => c.Id == s.ContentId)?.Title ?? "Unknown",
                        InteractionType = "Share",
                        InfluenceScore = _weights.ShareWeight
                    }));
                
                // Add recent comments
                recentInteractions.AddRange(user.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(3)
                    .Select(c => new ContentInteractionInfo
                    {
                        ContentId = c.ContentId,
                        ContentTitle = interactedContent.FirstOrDefault(ct => ct.Id == c.ContentId)?.Title ?? "Unknown",
                        InteractionType = "Comment",
                        InfluenceScore = _weights.CommentWeight
                    }));
                
                // Add recent likes
                recentInteractions.AddRange(user.Likes
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(3)
                    .Select(l => new ContentInteractionInfo
                    {
                        ContentId = l.ContentId,
                        ContentTitle = interactedContent.FirstOrDefault(c => c.Id == l.ContentId)?.Title ?? "Unknown",
                        InteractionType = "Like",
                        InfluenceScore = _weights.LikeWeight
                    }));

                // Take only the most recent/significant interactions
                recentInteractions = recentInteractions
                    .OrderByDescending(i => i.InfluenceScore)
                    .ThenByDescending(i => interactedContent.FirstOrDefault(c => c.Id == i.ContentId)?.CreatedAt ?? DateTime.MinValue)
                    .Take(5)
                    .ToList();

                return new PersonalizationFactors
                {
                    UserId = userId,
                    ShareFactor = shareFactor,
                    CommentFactor = commentFactor,
                    LikeFactor = likeFactor,
                    FollowFactor = followFactor,
                    PreferenceFactor = preferenceFactor,
                    InterestFactor = interestFactor,
                    ActivePreferences = user.Preferences,
                    ActiveInterests = user.Interests,
                    MostInfluentialFollows = influentialFollows,
                    RecentInteractions = recentInteractions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving personalization factors for user {UserId}", userId);
                throw;
            }
        }

        // Helper methods for building Elasticsearch queries
        private QueryContainer BuildBaseQuery(List<string> preferences, List<string> interests, List<int> followedUserIds)
        {
            var queries = new List<QueryContainer>();

            // Match content categories with user preferences
            if (preferences.Any())
            {
                queries.Add(new TermsQuery
                {
                    Field = Infer.Field<Content>(c => c.Categories),
                    Terms = preferences
                });
            }

            // Match content tags with user interests
            if (interests.Any())
            {
                queries.Add(new TermsQuery
                {
                    Field = Infer.Field<Content>(c => c.Tags),
                    Terms = interests
                });
            }

            // Match content from followed users
            if (followedUserIds.Any())
            {
                queries.Add(new TermsQuery
                {
                    Field = Infer.Field<Content>(c => c.CreatorId),
                    Terms = followedUserIds.Select(id => id.ToString())
                });
            }

            // If we have specific personalization signals, use them; otherwise, return recent content
            if (queries.Any())
            {
                return new BoolQuery
                {
                    Should = queries,
                    MinimumShouldMatch = 1
                };
            }
            else
            {
                // If no personalization signals, return recent content
                return new MatchAllQuery();
            }
        }

        private List<IScoreFunction> BuildScoringFunctions(PersonalizationFactors factors)
        {
            var functions = new List<IScoreFunction>();

            // Boost content from followed users
            if (factors.MostInfluentialFollows.Any())
            {
                foreach (var follow in factors.MostInfluentialFollows)
                {
                    functions.Add(new FieldValueFactorFunction
                    {
                        Field = Infer.Field<Content>(c => c.CreatorId),
                        Factor = factors.FollowFactor / 10,
                        Modifier = FieldValueFactorModifier.None,
                        Missing = 1.0,
                        Filter = new TermQuery
                        {
                            Field = Infer.Field<Content>(c => c.CreatorId),
                            Value = follow.UserId
                        }
                    });
                }
            }

            // Boost content matching user preferences
            if (factors.ActivePreferences.Any())
            {
                // Use ScriptScoreFunction instead of FunctionScoreQuery
                functions.Add(new ScriptScoreFunction
                {
                    Script = new InlineScript($"return {factors.PreferenceFactor / Math.Max(1, factors.ActivePreferences.Count())}"),
                    Filter = new TermsQuery
                    {
                        Field = Infer.Field<Content>(c => c.Categories),
                        Terms = factors.ActivePreferences
                    }
                });
            }

            // Boost content matching user interests
            if (factors.ActiveInterests.Any())
            {
                // Use ScriptScoreFunction instead of FunctionScoreQuery
                functions.Add(new ScriptScoreFunction
                {
                    Script = new InlineScript($"return {factors.InterestFactor / Math.Max(1, factors.ActiveInterests.Count())}"),
                    Filter = new TermsQuery
                    {
                        Field = Infer.Field<Content>(c => c.Tags),
                        Terms = factors.ActiveInterests
                    }
                });
            }

            // Add recency boost - favor newer content
            functions.Add(new GaussDateDecayFunction
            {
                Field = Infer.Field<Content>(c => c.CreatedAt),
                Origin = DateTime.UtcNow,
                Scale = "7d", // 7 days
                Decay = 0.5,
                // Remove Weight property as it's not supported in this version of NEST
                // Weight = 1.0
            });

            return functions;
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
