using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Threading;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;

namespace ElasticPersonalization.API.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["ElasticsearchSettings:Url"];
            var defaultIndex = configuration["ElasticsearchSettings:DefaultIndex"];
            
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Elasticsearch URL is not configured");
            }
            
            // Create connection pool with retry policy
            var uris = new Uri[] { new Uri(url) };
            var connectionPool = new StaticConnectionPool(uris);
            
            // Configure connection with retry settings
            var connectionSettings = new ConnectionSettings(connectionPool)
                .DefaultIndex(defaultIndex)
                .EnableDebugMode()
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromMinutes(2))
                .MaximumRetries(5)
                .RetryOnHttpError(false)
                .EnableApiVersioningHeader();
            
            // Add authentication if provided
            var username = configuration["ElasticsearchSettings:Username"];
            var password = configuration["ElasticsearchSettings:Password"];
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                connectionSettings.BasicAuthentication(username, password);
            }
            
            // Configure mappings for our entities
            connectionSettings
                .DefaultMappingFor<Core.Entities.Content>(m => m
                    .IndexName("content")
                    .IdProperty(c => c.Id));
            
            var client = new ElasticClient(connectionSettings);
            
            services.AddSingleton<IElasticClient>(client);
        }
        
        public static IElasticClient UseElasticsearch(this string elasticUri)
        {
            var settings = new ConnectionSettings(new Uri(elasticUri))
                .EnableDebugMode()
                .PrettyJson()
                .RequestTimeout(TimeSpan.FromSeconds(5))
                .SniffOnStartup(false)
                .SniffOnConnectionFault(false);
            
            return new ElasticClient(settings);
        }
        
        public static bool TryPingElasticsearch(this IElasticClient client, ILogger logger, int maxRetries = 5)
        {
            var retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    var pingResult = client.Ping();
                    if (pingResult.IsValid)
                    {
                        return true;
                    }
                    
                    logger.LogWarning("Elasticsearch ping failed: {Reason}", pingResult.DebugInformation);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error connecting to Elasticsearch (attempt {RetryCount}/{MaxRetries})", retryCount + 1, maxRetries);
                }
                
                retryCount++;
                
                if (retryCount < maxRetries)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2 * retryCount)); // Exponential backoff
                }
            }
            
            return false;
        }
    }
}