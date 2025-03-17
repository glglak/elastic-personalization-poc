using System;
using System.Collections.Generic;

namespace ElasticPersonalization.Core.Models
{
    public class ContentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public int CreatorId { get; set; }
        public string CreatorUsername { get; set; } = string.Empty;
        
        // Interaction stats
        public int ShareCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        
        // Personalization information (if applicable)
        public double? PersonalizationScore { get; set; }
    }
    
    public class CreateContentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public int CreatorId { get; set; }
    }
    
    public class UpdateContentDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Body { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Categories { get; set; }
    }
}
