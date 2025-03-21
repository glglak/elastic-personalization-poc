using System;

namespace ElasticPersonalization.Core.Entities
{
    public class UserLike
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ContentId { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Content Content { get; set; } = null!;
    }
}