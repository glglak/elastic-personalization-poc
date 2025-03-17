using System.Collections.Generic;

namespace ElasticPersonalization.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // User preferences and interests
        public List<string> Preferences { get; set; } = new();
        public List<string> Interests { get; set; } = new();
        
        // Navigation properties
        public virtual ICollection<UserShare> Shares { get; set; } = new List<UserShare>();
        public virtual ICollection<UserLike> Likes { get; set; } = new List<UserLike>();
        public virtual ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
        public virtual ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
        public virtual ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
    }
}
