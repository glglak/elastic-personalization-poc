#!/bin/bash

# Setup script for Elasticsearch Content Personalization POC
# This script is used to:
# 1. Start Docker containers
# 2. Initialize the SQL Server database
# 3. Initialize Elasticsearch indices

echo "Setting up Elasticsearch Content Personalization POC..."

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "Docker is not installed. Please install Docker and Docker Compose first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Start Docker containers
echo "Starting Docker containers..."
docker-compose up -d

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
sleep 30  # Give SQL Server some time to start

# Run SQL initialization script
echo "Initializing SQL Server database..."
docker-compose exec -T db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrongPassword! -i /var/opt/mssql/scripts/InitializeDB.sql

# Wait for Elasticsearch to be ready
echo "Waiting for Elasticsearch to be ready..."
sleep 20  # Give Elasticsearch some time to start

# Initialize Elasticsearch
echo "Initializing Elasticsearch..."
docker-compose exec -T app node /app/scripts/InitializeElasticsearch.js http://elasticsearch:9200 content

echo "Setup completed successfully!"
echo "The API is accessible at: http://localhost:5000"
echo "Kibana is accessible at: http://localhost:5601"
