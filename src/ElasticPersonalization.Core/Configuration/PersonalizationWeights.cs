namespace ElasticPersonalization.Core.Configuration
{
    public class PersonalizationWeights
    {
        public double ShareWeight { get; set; } = 5.0;
        public double CommentWeight { get; set; } = 4.0;
        public double LikeWeight { get; set; } = 3.0;
        public double FollowWeight { get; set; } = 4.5;
        public double PreferenceWeight { get; set; } = 2.0;
        public double InterestWeight { get; set; } = 1.5;
    }
}
