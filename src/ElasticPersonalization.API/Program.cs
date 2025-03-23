using ElasticPersonalization.API.Data;
using ElasticPersonalization.API.Extensions;
using ElasticPersonalization.Core.Interfaces;
using ElasticPersonalization.Infrastructure.Data;
using ElasticPersonalization.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Net;
using Nest;

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
    .AddElasticsearch("elasticsearch", options =>
    {
        var elasticUri = builder.Configuration["ElasticsearchSettings:Url"];
        options.UseElasticsearch(elasticUri);
    });

// Add Elasticsearch
builder.Services.AddElasticsearch(builder.Configuration);

// Register services
builder.Services.AddScoped<IUserInteractionService, UserInteractionService>();
builder.Services.AddScoped<IPersonalizationService, PersonalizationService>();
builder.Services.AddScoped<IContentService, ContentService>();

var app = builder.Build();

// Initialize the database on startup with retry logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Initializing database...");
    
    bool initialized = false;
    int retryCount = 0;
    int maxRetries = 10;
    
    while (!initialized && retryCount < maxRetries)
    {
        try
        {
            // Attempt to initialize the database
            DbInitializer.Initialize(services, logger);
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
            var elasticClient = services.GetRequiredService<IElasticClient>();
            var contentService = services.GetRequiredService<IContentService>();
            
            logger.LogInformation("Initializing Elasticsearch index...");
            await contentService.EnsureIndexExistsAsync();
            logger.LogInformation("Elasticsearch index initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing Elasticsearch index");
        }
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

app.Run();
