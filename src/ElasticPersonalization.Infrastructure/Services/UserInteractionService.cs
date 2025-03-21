using System;
using System.Linq;
using System.Threading.Tasks;
using ElasticPersonalization.Core.Entities;
using ElasticPersonalization.Core.Interfaces;
using ElasticPersonalization.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElasticPersonalization.Infrastructure.Services
{
    public class UserInteractionService : IUserInteractionService
    {
        private readonly ContentActionsDbContext _dbContext;
        private readonly ILogger<UserInteractionService> _logger;

        public UserInteractionService(ContentActionsDbContext dbContext, ILogger<UserInteractionService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UserShare> ShareContentAsync(int userId, int contentId)
        {
            try
            {
                // Check if user and content exist
                await EnsureUserExistsAsync(userId);
                await EnsureContentExistsAsync(contentId);

                // Check if already shared
                var existingShare = await _dbContext.Shares
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.ContentId == contentId);

                if (existingShare != null)
                {
                    return existingShare;
                }

                // Create new share
                var share = new UserShare
                {
                    UserId = userId,
                    ContentId = contentId,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Shares.Add(share);
                await _dbContext.SaveChangesAsync();

                return share;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing content {ContentId} by user {UserId}", contentId, userId);
                throw;
            }
        }

        public async Task<UserLike> LikeContentAsync(int userId, int contentId)
        {
            try
            {
                // Check if user and content exist
                await EnsureUserExistsAsync(userId);
                await EnsureContentExistsAsync(contentId);

                // Check if already liked
                var existingLike = await _dbContext.Likes
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.ContentId == contentId);

                if (existingLike != null)
                {
                    return existingLike;
                }

                // Create new like
                var like = new UserLike
                {
                    UserId = userId,
                    ContentId = contentId,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Likes.Add(like);
                await _dbContext.SaveChangesAsync();

                return like;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking content {ContentId} by user {UserId}", contentId, userId);
                throw;
            }
        }

        public async Task<UserComment> CommentOnContentAsync(int userId, int contentId, string commentText)
        {
            try
            {
                // Check if user and content exist
                await EnsureUserExistsAsync(userId);
                await EnsureContentExistsAsync(contentId);

                // Create new comment
                var comment = new UserComment
                {
                    UserId = userId,
                    ContentId = contentId,
                    CommentText = commentText,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Comments.Add(comment);
                await _dbContext.SaveChangesAsync();

                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error commenting on content {ContentId} by user {UserId}", contentId, userId);
                throw;
            }
        }

        public async Task<UserFollow> FollowUserAsync(int userId, int followedUserId)
        {
            try
            {
                // Check if both users exist
                await EnsureUserExistsAsync(userId);
                await EnsureUserExistsAsync(followedUserId);

                // Prevent self-following
                if (userId == followedUserId)
                {
                    throw new InvalidOperationException("User cannot follow themselves");
                }

                // Check if already following
                var existingFollow = await _dbContext.Follows
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.FollowedUserId == followedUserId);

                if (existingFollow != null)
                {
                    return existingFollow;
                }

                // Create new follow
                var follow = new UserFollow
                {
                    UserId = userId,
                    FollowedUserId = followedUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Follows.Add(follow);
                await _dbContext.SaveChangesAsync();

                return follow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following user {FollowedUserId} by user {UserId}", followedUserId, userId);
                throw;
            }
        }

        public async Task RemoveShareAsync(int userId, int contentId)
        {
            try
            {
                var share = await _dbContext.Shares
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.ContentId == contentId);

                if (share != null)
                {
                    _dbContext.Shares.Remove(share);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing share of content {ContentId} by user {UserId}", contentId, userId);
                throw;
            }
        }

        public async Task RemoveLikeAsync(int userId, int contentId)
        {
            try
            {
                var like = await _dbContext.Likes
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.ContentId == contentId);

                if (like != null)
                {
                    _dbContext.Likes.Remove(like);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing like of content {ContentId} by user {UserId}", contentId, userId);
                throw;
            }
        }

        public async Task RemoveCommentAsync(int commentId)
        {
            try
            {
                var comment = await _dbContext.Comments.FindAsync(commentId);

                if (comment != null)
                {
                    _dbContext.Comments.Remove(comment);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task UnfollowUserAsync(int userId, int followedUserId)
        {
            try
            {
                var follow = await _dbContext.Follows
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.FollowedUserId == followedUserId);

                if (follow != null)
                {
                    _dbContext.Follows.Remove(follow);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfollowing user {FollowedUserId} by user {UserId}", followedUserId, userId);
                throw;
            }
        }

        public async Task<User> AddUserPreferenceAsync(int userId, string preference)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                if (!user.Preferences.Contains(preference))
                {
                    user.Preferences.Add(preference);
                    await _dbContext.SaveChangesAsync();
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding preference {Preference} to user {UserId}", preference, userId);
                throw;
            }
        }

        public async Task<User> AddUserInterestAsync(int userId, string interest)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                if (!user.Interests.Contains(interest))
                {
                    user.Interests.Add(interest);
                    await _dbContext.SaveChangesAsync();
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding interest {Interest} to user {UserId}", interest, userId);
                throw;
            }
        }

        public async Task<User> RemoveUserPreferenceAsync(int userId, string preference)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                if (user.Preferences.Contains(preference))
                {
                    user.Preferences.Remove(preference);
                    await _dbContext.SaveChangesAsync();
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing preference {Preference} from user {UserId}", preference, userId);
                throw;
            }
        }

        public async Task<User> RemoveUserInterestAsync(int userId, string interest)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                if (user.Interests.Contains(interest))
                {
                    user.Interests.Remove(interest);
                    await _dbContext.SaveChangesAsync();
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing interest {Interest} from user {UserId}", interest, userId);
                throw;
            }
        }

        // Helper methods
        private async Task EnsureUserExistsAsync(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }
        }

        private async Task EnsureContentExistsAsync(int contentId)
        {
            var content = await _dbContext.Content.FindAsync(contentId);
            if (content == null)
            {
                throw new ArgumentException($"Content with ID {contentId} not found");
            }
        }
    }
}