using System;
using System.Threading.Tasks;
using ElasticPersonalization.API.Models;
using ElasticPersonalization.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;

namespace ElasticPersonalization.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ContentActionsDbContext _dbContext;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            ContentActionsDbContext dbContext,
            IElasticClient elasticClient,
            ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var result = new HealthCheckResponse
                {
                    Timestamp = DateTime.UtcNow,
                    Components = new ComponentsStatus
                    {
                        Api = new ComponentStatus { Status = "Healthy" },
                        Database = await CheckDatabaseHealthAsync(),
                        Elasticsearch = await CheckElasticsearchHealthAsync()
                    }
                };

                if (result.Components.Database.Status != "Healthy" || 
                    result.Components.Elasticsearch.Status != "Healthy")
                {
                    result.Status = "Unhealthy";
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health status");
                return StatusCode(StatusCodes.Status500InternalServerError, new HealthCheckResponse
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Components = new ComponentsStatus
                    {
                        Api = new ComponentStatus 
                        { 
                            Status = "Unhealthy",
                            Message = ex.Message
                        }
                    }
                });
            }
        }

        private async Task<ComponentStatus> CheckDatabaseHealthAsync()
        {
            try
            {
                // Check if we can connect to the database
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return new ComponentStatus
                    {
                        Status = "Unhealthy",
                        Message = "Cannot connect to database"
                    };
                }

                return new ComponentStatus
                {
                    Status = "Healthy",
                    Message = "Database connection successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return new ComponentStatus
                {
                    Status = "Unhealthy",
                    Message = $"Database health check failed: {ex.Message}"
                };
            }
        }

        private async Task<ComponentStatus> CheckElasticsearchHealthAsync()
        {
            try
            {
                // Check if Elasticsearch is available
                var healthResponse = await _elasticClient.Cluster.HealthAsync();
                
                if (!healthResponse.IsValid)
                {
                    return new ComponentStatus
                    {
                        Status = "Unhealthy",
                        Message = $"Elasticsearch health check failed: {healthResponse.DebugInformation}"
                    };
                }

                return new ComponentStatus
                {
                    Status = "Healthy",
                    Message = $"Elasticsearch status: {healthResponse.Status}",
                    ClusterName = healthResponse.ClusterName,
                    NumberOfNodes = healthResponse.NumberOfNodes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch health check failed");
                return new ComponentStatus
                {
                    Status = "Unhealthy",
                    Message = $"Elasticsearch health check failed: {ex.Message}"
                };
            }
        }
    }
}
