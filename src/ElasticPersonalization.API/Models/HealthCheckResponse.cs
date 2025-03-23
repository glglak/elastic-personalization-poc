using System;
using System.Collections.Generic;

namespace ElasticPersonalization.API.Models
{
    /// <summary>
    /// Response model for the health check endpoint
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Overall status of the application health
        /// </summary>
        public string Status { get; set; } = "Healthy";
        
        /// <summary>
        /// Timestamp when the health check was performed
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Status of individual components
        /// </summary>
        public ComponentsStatus Components { get; set; } = new ComponentsStatus();
    }

    /// <summary>
    /// Contains the status of all application components
    /// </summary>
    public class ComponentsStatus
    {
        /// <summary>
        /// Status of the API itself
        /// </summary>
        public ComponentStatus Api { get; set; } = new ComponentStatus { Status = "Healthy" };
        
        /// <summary>
        /// Status of the database connection
        /// </summary>
        public ComponentStatus Database { get; set; } = new ComponentStatus();
        
        /// <summary>
        /// Status of the Elasticsearch connection
        /// </summary>
        public ComponentStatus Elasticsearch { get; set; } = new ComponentStatus();
    }

    /// <summary>
    /// Represents the health status of a single component
    /// </summary>
    public class ComponentStatus
    {
        /// <summary>
        /// Status of the component: "Healthy", "Unhealthy", "Degraded"
        /// </summary>
        public string Status { get; set; } = "Unhealthy";
        
        /// <summary>
        /// Descriptive message about the component's health
        /// </summary>
        public string Message { get; set; } = "Not checked";
        
        /// <summary>
        /// Optional additional details about the component
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }
}