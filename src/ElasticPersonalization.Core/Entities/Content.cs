using System;
using System.Collections.Generic;

namespace ElasticPersonalization.Core.Entities
{
    public class Content
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> Tags { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public int CreatorId { get; set; }
        
        // Navigation properties
        public virtual User Creator { get; set; } = null!;
        public virtual ICollection<UserShare> Shares { get; set; } = new List<UserShare>();
        public virtual ICollection<UserLike> Likes { get; set; } = new List<UserLike>();
        public virtual ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
    }
}
