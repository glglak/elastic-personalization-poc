using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;

namespace ElasticPersonalization.API.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["ElasticsearchSettings:Url"];
            var defaultIndex = configuration["ElasticsearchSettings:DefaultIndex"];
            
            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex);
            
            // Add authentication if provided
            var username = configuration["ElasticsearchSettings:Username"];
            var password = configuration["ElasticsearchSettings:Password"];
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                settings.BasicAuthentication(username, password);
            }
            
            // Configure mappings for our entities
            settings
                .DefaultMappingFor<Core.Entities.Content>(m => m
                    .IndexName("content")
                    .IdProperty(c => c.Id));
            
            var client = new ElasticClient(settings);
            
            services.AddSingleton<IElasticClient>(client);
        }
    }
}