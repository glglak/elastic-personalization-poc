using System;

namespace ElasticPersonalization.API.Models
{
    public class HealthCheckResponse
    {
        public string Status { get; set; } = "Healthy";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public ComponentsStatus Components { get; set; } = new ComponentsStatus();
    }

    public class ComponentsStatus
    {
        public ComponentStatus Api { get; set; } = new ComponentStatus { Status = "Healthy" };
        public ComponentStatus Database { get; set; } = new ComponentStatus();
        public ComponentStatus Elasticsearch { get; set; } = new ComponentStatus();
    }

    public class ComponentStatus
    {
        public string Status { get; set; } = "Unhealthy";
        public string Message { get; set; } = "Not checked";
        public string ClusterName { get; set; }
        public int? NumberOfNodes { get; set; }
    }
}
