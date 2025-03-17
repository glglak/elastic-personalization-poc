#!/bin/bash

# Default connection string
CONNECTION_STRING="Server=localhost;Database=ContentActions;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;"

# Parse command-line arguments
while [[ $# -gt 0 ]]; do
    key="$1"
    case $key in
        --connection-string)
        CONNECTION_STRING="$2"
        shift # past argument
        shift # past value
        ;;
        *)
        shift # past argument
        ;;
    esac
done

echo -e "\e[32mRunning database migrations and seeding data...\e[0m"

# Set the working directory to the Infrastructure project
WORKING_DIR="$(pwd)/src/ElasticPersonalization.Infrastructure"
echo -e "\e[36mWorking directory: $WORKING_DIR\e[0m"

# Check if the directory exists
if [ ! -d "$WORKING_DIR" ]; then
    echo -e "\e[31mError: The working directory does not exist. Make sure you're running this script from the root of the repository.\e[0m"
    exit 1
fi

# Set up environment variables
export ASPNETCORE_ENVIRONMENT="Development"
export ConnectionStrings__ContentActionsConnection="$CONNECTION_STRING"

# Change to the Infrastructure project directory
pushd "$WORKING_DIR" > /dev/null

# Ensure EF Core tools are installed
echo -e "\e[36mChecking for EF Core tools...\e[0m"
if ! dotnet tool list --global | grep -q "dotnet-ef"; then
    echo -e "\e[33mInstalling EF Core tools...\e[0m"
    dotnet tool install --global dotnet-ef
    if [ $? -ne 0 ]; then
        echo -e "\e[31mError installing EF Core tools. Please install manually with 'dotnet tool install --global dotnet-ef'\e[0m"
        exit 1
    fi
fi

# Apply migrations
echo -e "\e[36mApplying database migrations...\e[0m"
dotnet ef database update

if [ $? -ne 0 ]; then
    echo -e "\e[31mError applying migrations. Please check your connection string and try again.\e[0m"
    popd > /dev/null
    exit 1
fi

echo -e "\e[32mMigrations applied successfully!\e[0m"

# Run the seeding program
echo -e "\e[36mSeeding the database and Elasticsearch...\e[0m"

SEED_PROGRAM_PATH="../ElasticPersonalization.API/SeedDataProgram.cs"

# Check if the seed program exists, create it if it doesn't
if [ ! -f "$SEED_PROGRAM_PATH" ]; then
    echo -e "\e[33mCreating seed program...\e[0m"
    
    cat > "$SEED_PROGRAM_PATH" << 'EOF'
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
EOF
fi

# Run the seed program
pushd "../ElasticPersonalization.API" > /dev/null
dotnet run --project ElasticPersonalization.API.csproj SeedDataProgram.cs

if [ $? -ne 0 ]; then
    echo -e "\e[31mError seeding data. Please check the error messages above.\e[0m"
    popd > /dev/null
    popd > /dev/null
    exit 1
fi

echo -e "\e[32mData seeded successfully!\e[0m"
popd > /dev/null

# Return to the original directory
popd > /dev/null

echo -e "\e[32mDatabase setup completed successfully!\e[0m"
