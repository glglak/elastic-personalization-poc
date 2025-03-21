using System;
using System.Collections.Generic;

namespace ElasticPersonalization.Core.Entities
{
    public class Content
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Add Body property (same as ContentText, but required by services)
        public string Body { get; set; } = string.Empty;
        
        // Keep ContentText for backward compatibility
        public string ContentText { get; set; } = string.Empty;
        
        public string ContentType { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        
        // Add CreatorId and Creator properties
        public int CreatorId { get; set; }
        public virtual User Creator { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<UserShare> Shares { get; set; } = new List<UserShare>();
        public virtual ICollection<UserLike> Likes { get; set; } = new List<UserLike>();
        public virtual ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
    }
}