using System.Threading.Tasks;
using ElasticPersonalization.Core.Entities;

namespace ElasticPersonalization.Core.Interfaces
{
    public interface IUserInteractionService
    {
        Task<UserShare> ShareContentAsync(int userId, string contentId);
        Task<UserLike> LikeContentAsync(int userId, string contentId);
        Task<UserComment> CommentOnContentAsync(int userId, string contentId, string commentText);
        Task<UserFollow> FollowUserAsync(int userId, int followedUserId);
        
        Task RemoveShareAsync(int userId, string contentId);
        Task RemoveLikeAsync(int userId, string contentId);
        Task RemoveCommentAsync(int commentId);
        Task UnfollowUserAsync(int userId, int followedUserId);
        
        Task<User> AddUserPreferenceAsync(int userId, string preference);
        Task<User> AddUserInterestAsync(int userId, string interest);
        Task<User> RemoveUserPreferenceAsync(int userId, string preference);
        Task<User> RemoveUserInterestAsync(int userId, string interest);
    }
}
