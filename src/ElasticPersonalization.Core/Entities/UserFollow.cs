using System;

namespace ElasticPersonalization.Core.Entities
{
    public class UserFollow
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FollowedUserId { get; set; }
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User FollowedUser { get; set; } = null!;
    }
}