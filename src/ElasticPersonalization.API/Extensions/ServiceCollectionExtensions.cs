using ElasticPersonalization.Core.Configuration;
using Elasticsearch.Net;
using Nest;

namespace ElasticPersonalization.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
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
