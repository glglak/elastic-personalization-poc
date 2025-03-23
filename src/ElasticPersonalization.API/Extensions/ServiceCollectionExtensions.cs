using ElasticPersonalization.Core.Configuration;
using Elasticsearch.Net;
using Nest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ElasticPersonalization.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // Rename the method to avoid ambiguity
        public static void ConfigureElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticConfig = configuration.GetSection("ElasticsearchSettings").Get<ElasticsearchSettings>() ?? 
                throw new InvalidOperationException("Elasticsearch settings are missing from configuration");
            
            var connectionPool = new SingleNodeConnectionPool(new Uri(elasticConfig.Url));
            var connectionSettings = new ConnectionSettings(connectionPool)
                .DefaultIndex(elasticConfig.DefaultIndex)
                .BasicAuthentication(elasticConfig.Username, elasticConfig.Password);

            var client = new ElasticClient(connectionSettings);
            services.AddSingleton<IElasticClient>(client);
        }
    }
}
