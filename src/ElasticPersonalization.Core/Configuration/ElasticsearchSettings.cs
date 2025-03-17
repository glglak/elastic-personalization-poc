namespace ElasticPersonalization.Core.Configuration
{
    public class ElasticsearchSettings
    {
        public string Url { get; set; } = string.Empty;
        public string DefaultIndex { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
