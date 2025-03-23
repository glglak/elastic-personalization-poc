using ElasticPersonalization.API.Data;
using ElasticPersonalization.API.Extensions;
using ElasticPersonalization.Core.Interfaces;
using ElasticPersonalization.Infrastructure.Data;
using ElasticPersonalization.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Nest;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context with retry logic
var connectionString = builder.Configuration.GetConnectionString("ContentActionsConnection");
builder.Services.AddDbContext<ContentActionsDbContext>(options => 
{
    options.UseSqlServer(connectionString, 
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ContentActionsDbContext>("database")
    .AddCheck("elasticsearch", () => {
        var elasticUri = builder.Configuration["ElasticsearchSettings:Url"];
        try
        {
            var client = ElasticsearchExtensions.UseElasticsearch(elasticUri);
            var result = client.Ping();
            if (result.IsValid)
            {
                return HealthCheckResult.Healthy("Elasticsearch is healthy");
            }
            return HealthCheckResult.Degraded($"Elasticsearch ping failed: {result.DebugInformation}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Elasticsearch check failed: {ex.Message}");
        }
    });

// Add Elasticsearch using our custom extension method
builder.Services.AddElasticsearch(builder.Configuration);

// Register services
builder.Services.AddScoped<IUserInteractionService, UserInteractionService>();
builder.Services.AddScoped<IPersonalizationService, PersonalizationService>();
builder.Services.AddScoped<IContentService, ContentService>();

var app = builder.Build();

// Initialize the database on startup with retry logic
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Initializing database...");

bool initialized = false;
int retryCount = 0;
int maxRetries = 10;

while (!initialized && retryCount < maxRetries)
{
    try
    {
        // Attempt to initialize the database
        DbInitializer.Initialize(app.Services, logger);
        initialized = true;
        logger.LogInformation("Database initialized successfully");
    }
    catch (SqlException ex) when (ex.Number == 53 || ex.Number == 40 || ex.Number == 18456)
    {
        // SQL Server is not available yet - retry
        retryCount++;
        logger.LogWarning($"Database connection failed (attempt {retryCount}/{maxRetries}): {ex.Message}");
        
        if (retryCount < maxRetries)
        {
            logger.LogInformation($"Waiting before retry...");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
        else
        {
            logger.LogError($"Failed to connect to database after {maxRetries} attempts");
            throw;
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

// Initialize Elasticsearch index if database is ready
if (initialized)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var elasticClient = scope.ServiceProvider.GetRequiredService<IElasticClient>();
            var contentService = scope.ServiceProvider.GetRequiredService<IContentService>();
            
            logger.LogInformation("Initializing Elasticsearch index...");
            contentService.EnsureIndexExistsAsync().GetAwaiter().GetResult();
            logger.LogInformation("Elasticsearch index initialized successfully");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing Elasticsearch index");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
