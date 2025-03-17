param (
    [string]$ConnectionString = "Server=localhost;Database=ContentActions;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;"
)

Write-Host "Running database migrations and seeding data..." -ForegroundColor Green

# Set the working directory to the Infrastructure project
$workingDir = Join-Path (Get-Location) "src\ElasticPersonalization.Infrastructure"
Write-Host "Working directory: $workingDir" -ForegroundColor Cyan

# Check if the directory exists
if (-Not (Test-Path $workingDir)) {
    Write-Host "Error: The working directory does not exist. Make sure you're running this script from the root of the repository." -ForegroundColor Red
    exit 1
}

# Set up environment variables
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__ContentActionsConnection = $ConnectionString

# Change to the Infrastructure project directory
Push-Location $workingDir

try {
    # Ensure EF Core tools are installed
    Write-Host "Checking for EF Core tools..." -ForegroundColor Cyan
    $toolsInstalled = dotnet tool list --global | Select-String "dotnet-ef"
    
    if (-Not $toolsInstalled) {
        Write-Host "Installing EF Core tools..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-ef
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error installing EF Core tools. Please install manually with 'dotnet tool install --global dotnet-ef'" -ForegroundColor Red
            exit 1
        }
    }
    
    # Apply migrations
    Write-Host "Applying database migrations..." -ForegroundColor Cyan
    dotnet ef database update
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error applying migrations. Please check your connection string and try again." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Migrations applied successfully!" -ForegroundColor Green
    
    # Run the seeding program
    Write-Host "Seeding the database and Elasticsearch..." -ForegroundColor Cyan
    
    $seedProgramPath = Join-Path (Get-Location) "..\ElasticPersonalization.API\SeedDataProgram.cs"
    
    # Check if the seed program exists, create it if it doesn't
    if (-Not (Test-Path $seedProgramPath)) {
        Write-Host "Creating seed program..." -ForegroundColor Yellow
        
        $seedProgram = @"
using ElasticPersonalization.API.Extensions;
using ElasticPersonalization.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ElasticPersonalization.API
{
    public class SeedDataProgram
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting data seeding program...");
            
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            // Create service collection
            var services = new ServiceCollection();
            
            // Configure logging
            services.AddLogging(builder => 
            {
                builder.AddConsole();
                builder.AddDebug();
            });
            
            // Add database context
            services.AddDbContext<ContentActionsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("ContentActionsConnection")));
            
            // Add Elasticsearch
            services.AddElasticsearch(configuration);
            
            // Add seed data service
            services.AddTransient<SeedData>();
            
            // Build service provider
            var serviceProvider = services.BuildServiceProvider();
            
            try 
            {
                // Get seed data service
                var seedData = serviceProvider.GetRequiredService<SeedData>();
                
                // Run seed data
                await seedData.SeedAllAsync();
                
                Console.WriteLine("Data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during data seeding: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
"@
        Set-Content -Path $seedProgramPath -Value $seedProgram
    }
    
    # Run the seed program
    Push-Location "..\ElasticPersonalization.API"
    try {
        dotnet run --project ElasticPersonalization.API.csproj SeedDataProgram.cs
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error seeding data. Please check the error messages above." -ForegroundColor Red
            exit 1
        }
        
        Write-Host "Data seeded successfully!" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}
finally {
    # Return to the original directory
    Pop-Location
}

Write-Host "Database setup completed successfully!" -ForegroundColor Green
