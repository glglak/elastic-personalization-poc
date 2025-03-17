using System.Collections.Generic;

namespace ElasticPersonalization.Core.Models
{
    public class PersonalizationFactors
    {
        public int UserId { get; set; }
        
        // Weighted factors
        public double ShareFactor { get; set; }
        public double CommentFactor { get; set; }
        public double LikeFactor { get; set; }
        public double FollowFactor { get; set; }
        public double PreferenceFactor { get; set; }
        public double InterestFactor { get; set; }
        
        // Supportive data
        public IEnumerable<string> ActivePreferences { get; set; } = new List<string>();
        public IEnumerable<string> ActiveInterests { get; set; } = new List<string>();
        public IEnumerable<UserFollowInfo> MostInfluentialFollows { get; set; } = new List<UserFollowInfo>();
        public IEnumerable<ContentInteractionInfo> RecentInteractions { get; set; } = new List<ContentInteractionInfo>();
    }
    
    public class UserFollowInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public double InfluenceScore { get; set; }
    }
    
    public class ContentInteractionInfo
    {
        public string ContentId { get; set; } = string.Empty;
        public string ContentTitle { get; set; } = string.Empty;
        public string InteractionType { get; set; } = string.Empty; // Share, Like, Comment
        public double InfluenceScore { get; set; }
    }
}
