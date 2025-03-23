#!/bin/bash
set -e

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start up..."
/app/scripts/wait-for-it.sh db:1433 -t 90

# Wait for Elasticsearch to be ready
echo "Waiting for Elasticsearch to start up..."
/app/scripts/wait-for-it.sh elasticsearch:9200 -t 90

# Start the .NET application
echo "Starting API application..."
exec dotnet ElasticPersonalization.API.dll
