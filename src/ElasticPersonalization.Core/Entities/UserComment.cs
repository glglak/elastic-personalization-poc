using System;

namespace ElasticPersonalization.Core.Entities
{
    public class UserComment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ContentId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Content Content { get; set; } = null!;
    }
}