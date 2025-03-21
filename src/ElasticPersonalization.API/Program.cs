using ElasticPersonalization.API.Data;
using ElasticPersonalization.API.Extensions;
using ElasticPersonalization.Core.Interfaces;
using ElasticPersonalization.Infrastructure.Data;
using ElasticPersonalization.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<ContentActionsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ContentActionsConnection")));

// Add Elasticsearch
builder.Services.AddElasticsearch(builder.Configuration);

// Register services
builder.Services.AddScoped<IUserInteractionService, UserInteractionService>();
builder.Services.AddScoped<IPersonalizationService, PersonalizationService>();
builder.Services.AddScoped<IContentService, ContentService>();

var app = builder.Build();

// Initialize the database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Initializing database...");
    DbInitializer.Initialize(services, logger);
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
