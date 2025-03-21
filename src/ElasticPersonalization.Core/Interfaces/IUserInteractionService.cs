using System.Threading.Tasks;
using ElasticPersonalization.Core.Entities;

namespace ElasticPersonalization.Core.Interfaces
{
    public interface IUserInteractionService
    {
        Task<UserShare> ShareContentAsync(int userId, int contentId);
        Task<UserLike> LikeContentAsync(int userId, int contentId);
        Task<UserComment> CommentOnContentAsync(int userId, int contentId, string commentText);
        Task<UserFollow> FollowUserAsync(int userId, int followedUserId);
        
        Task RemoveShareAsync(int userId, int contentId);
        Task RemoveLikeAsync(int userId, int contentId);
        Task RemoveCommentAsync(int commentId);
        Task UnfollowUserAsync(int userId, int followedUserId);
        
        Task<User> AddUserPreferenceAsync(int userId, string preference);
        Task<User> AddUserInterestAsync(int userId, string interest);
        Task<User> RemoveUserPreferenceAsync(int userId, string preference);
        Task<User> RemoveUserInterestAsync(int userId, string interest);
    }
}