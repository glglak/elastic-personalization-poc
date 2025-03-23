using System;
using System.Threading.Tasks;
using ElasticPersonalization.API.Models;
using ElasticPersonalization.Core.Interfaces;
using ElasticPersonalization.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System.Collections.Generic;
using System.Linq;

namespace ElasticPersonalization.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ContentActionsDbContext _dbContext;
        private readonly IElasticClient _elasticClient;
        private readonly IContentService _contentService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            ContentActionsDbContext dbContext,
            IElasticClient elasticClient,
            IContentService contentService,
            ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _elasticClient = elasticClient;
            _contentService = contentService;
            _logger = logger;
        }

        /// <summary>
        /// Checks the health of the application and its dependencies
        /// </summary>
        /// <returns>Health status of the application</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HealthCheckResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(HealthCheckResponse))]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var databaseStatus = await CheckDatabaseHealthAsync();
                var elasticsearchStatus = await CheckElasticsearchHealthAsync();
                
                var result = new HealthCheckResponse
                {
                    Timestamp = DateTime.UtcNow,
                    Components = new ComponentsStatus
                    {
                        Api = new ComponentStatus { Status = "Healthy" },
                        Database = databaseStatus,
                        Elasticsearch = elasticsearchStatus
                    }
                };

                // Set overall status based on component health
                result.Status = (databaseStatus.Status == "Healthy" && 
                              elasticsearchStatus.Status == "Healthy") 
                             ? "Healthy" : "Unhealthy";

                // Return appropriate HTTP status code
                if (result.Status != "Healthy")
                {
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
                            Message = ex.Message,
                            Details = new Dictionary<string, object>
                            {
                                ["exception"] = ex.GetType().Name,
                                ["stackTrace"] = ex.StackTrace
                            }
                        }
                    }
                });
            }
        }

        [HttpGet("database")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckDatabaseHealth()
        {
            var status = await CheckDatabaseHealthAsync();
            
            if (status.Status != "Healthy")
            {
                return StatusCode(StatusCodes.Status500InternalServerError, status);
            }
            
            return Ok(status);
        }

        [HttpGet("elasticsearch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckElasticsearchHealth()
        {
            var status = await CheckElasticsearchHealthAsync();
            
            if (status.Status != "Healthy")
            {
                return StatusCode(StatusCodes.Status500InternalServerError, status);
            }
            
            return Ok(status);
        }

        [HttpPost("elasticsearch/initialize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitializeElasticsearch()
        {
            try
            {
                var result = await _contentService.EnsureIndexExistsAsync();
                
                if (result)
                {
                    return Ok(new { Status = "Success", Message = "Elasticsearch index created or verified successfully" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, 
                        new { Status = "Failed", Message = "Failed to create Elasticsearch index" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Elasticsearch");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Status = "Failed", Message = $"Error initializing Elasticsearch: {ex.Message}" });
            }
        }

        [HttpPost("elasticsearch/reindex")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReindexContent()
        {
            try
            {
                await _contentService.ReindexAllContentAsync();
                return Ok(new { Status = "Success", Message = "Content reindexing initiated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reindexing content");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Status = "Failed", Message = $"Error reindexing content: {ex.Message}" });
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

                // Get basic database statistics
                var statistics = new Dictionary<string, object>();
                
                try
                {
                    statistics["userCount"] = await _dbContext.Users.CountAsync();
                    statistics["contentCount"] = await _dbContext.Content.CountAsync();
                    statistics["interactionsCount"] = await _dbContext.Likes.CountAsync() + 
                                                    await _dbContext.Comments.CountAsync() + 
                                                    await _dbContext.Shares.CountAsync() + 
                                                    await _dbContext.Follows.CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get database statistics");
                    statistics["error"] = "Failed to get database statistics";
                }

                return new ComponentStatus
                {
                    Status = "Healthy",
                    Message = "Database connection successful",
                    Details = statistics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return new ComponentStatus
                {
                    Status = "Unhealthy",
                    Message = $"Database health check failed: {ex.Message}",
                    Details = new Dictionary<string, object>
                    {
                        ["exception"] = ex.GetType().Name,
                        ["connectionString"] = MaskConnectionString(_dbContext.Database.GetConnectionString())
                    }
                };
            }
        }

        private async Task<ComponentStatus> CheckElasticsearchHealthAsync()
        {
            try
            {
                // First check using our custom ping method
                var canPing = await _contentService.PingElasticsearchAsync();
                
                if (!canPing)
                {
                    return new ComponentStatus
                    {
                        Status = "Unhealthy",
                        Message = "Cannot ping Elasticsearch"
                    };
                }
                
                // Get more detailed health information from the cluster
                var healthResponse = await _elasticClient.Cluster.HealthAsync();
                
                if (!healthResponse.IsValid)
                {
                    return new ComponentStatus
                    {
                        Status = "Unhealthy",
                        Message = $"Elasticsearch health check failed: {healthResponse.DebugInformation}"
                    };
                }

                // Check if the index exists
                var indexExists = await _elasticClient.Indices.ExistsAsync(_elasticClient.ConnectionSettings.DefaultIndex);
                
                var details = new Dictionary<string, object>
                {
                    ["clusterName"] = healthResponse.ClusterName,
                    ["status"] = healthResponse.Status.ToString(),
                    ["numberOfNodes"] = healthResponse.NumberOfNodes,
                    ["numberOfDataNodes"] = healthResponse.NumberOfDataNodes,
                    ["activePrimaryShards"] = healthResponse.ActivePrimaryShards,
                    ["activeShards"] = healthResponse.ActiveShards,
                    ["relocatingShards"] = healthResponse.RelocatingShards,
                    ["initializingShards"] = healthResponse.InitializingShards,
                    ["unassignedShards"] = healthResponse.UnassignedShards,
                    ["indexExists"] = indexExists.Exists
                };

                // If the cluster status is red, mark as unhealthy
                if (healthResponse.Status == Elasticsearch.Net.Health.Red)
                {
                    return new ComponentStatus
                    {
                        Status = "Unhealthy",
                        Message = $"Elasticsearch cluster status is red",
                        Details = details
                    };
                }

                // Add index count if index exists
                if (indexExists.Exists)
                {
                    try
                    {
                        var countResponse = await _elasticClient.CountAsync<Content>(c => c
                            .Index(_elasticClient.ConnectionSettings.DefaultIndex));
                        
                        if (countResponse.IsValid)
                        {
                            details["documentCount"] = countResponse.Count;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get document count");
                        details["countError"] = "Failed to get document count";
                    }
                }
                else
                {
                    // Add a warning if the index doesn't exist
                    details["warning"] = $"Index '{_elasticClient.ConnectionSettings.DefaultIndex}' does not exist";
                }

                return new ComponentStatus
                {
                    Status = "Healthy",
                    Message = $"Elasticsearch status: {healthResponse.Status}",
                    Details = details
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Elasticsearch health check failed");
                return new ComponentStatus
                {
                    Status = "Unhealthy",
                    Message = $"Elasticsearch health check failed: {ex.Message}",
                    Details = new Dictionary<string, object>
                    {
                        ["exception"] = ex.GetType().Name,
                        ["elasticsearchUri"] = _elasticClient.ConnectionSettings.ConnectionPool.Nodes.FirstOrDefault()?.Uri.ToString()
                    }
                };
            }
        }
        
        /// <summary>
        /// Masks a connection string to hide sensitive information
        /// </summary>
        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return "(null)";
            }
            
            try
            {
                // Replace password with asterisks
                var parts = connectionString.Split(';')
                    .Select(part => 
                    {
                        if (part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) || 
                            part.StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
                        {
                            return part.Split('=')[0] + "=********";
                        }
                        return part;
                    });
                    
                return string.Join(";", parts);
            }
            catch
            {
                // If parsing fails, return with all specific values masked
                return connectionString.Replace(connectionString, "********");
            }
        }
    }
}
