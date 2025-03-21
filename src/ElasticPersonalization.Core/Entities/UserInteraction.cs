using System;

namespace ElasticPersonalization.Core.Entities
{
    // Base class for user interactions
    public abstract class UserInteraction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }

    // User Share interaction
    public class UserShare : UserInteraction
    {
        // Changed from string to int to match Content.Id type
        public int ContentId { get; set; }
        
        // Navigation property
        public virtual Content Content { get; set; } = null!;
    }

    // User Like interaction
    public class UserLike : UserInteraction
    {
        // Changed from string to int to match Content.Id type
        public int ContentId { get; set; }
        
        // Navigation property
        public virtual Content Content { get; set; } = null!;
    }

    // User Comment interaction
    public class UserComment : UserInteraction
    {
        // Changed from string to int to match Content.Id type
        public int ContentId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        
        // Navigation property
        public virtual Content Content { get; set; } = null!;
    }

    // User Follow interaction
    public class UserFollow : UserInteraction
    {
        public int FollowedUserId { get; set; }
        
        // Navigation property - the user being followed
        public virtual User FollowedUser { get; set; } = null!;
    }
}