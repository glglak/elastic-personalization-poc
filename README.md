# Elasticsearch Content Personalization POC

A proof of concept in .NET Core 8 that demonstrates how to implement a personalized content feed based on user interactions with Elasticsearch and a relational database for content actions.

## 📋 Documentation

**[View the full documentation on GitHub Pages](https://glglak.github.io/elastic-personalization-poc/)**

## 🔍 Overview

This proof of concept demonstrates how to create a personalized content feed by leveraging:

- Elasticsearch for efficient content search and scoring
- SQL Server for storing user interactions
- Configurable personalization weights for different user actions

The system personalizes content based on a hierarchy of user interactions:

1. Share actions (highest weight)
2. Comment actions 
3. Like actions
4. Follow relationships
5. User preferences
6. User interests (lowest weight)

## 🏗️ Architecture

The architecture follows a clean, layered approach:

- **API Layer**: RESTful endpoints for content, personalization, and user interactions
- **Core Layer**: Domain entities, interfaces, and business models
- **Infrastructure Layer**: Service implementations and data access
- **Data Stores**: SQL Server for user interactions and Elasticsearch for content indexing

## 🚀 Getting Started

### Using Docker for Windows (Without Docker Compose)

1. **Prerequisites**:
   - [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop/) installed and running
   - [Git for Windows](https://git-scm.com/download/win) for cloning the repository
   - WSL 2 enabled (recommended for better performance)

2. **Clone the repository**:
   ```powershell
   git clone https://github.com/glglak/elastic-personalization-poc.git
   cd elastic-personalization-poc
   ```

3. **Create a Docker network**:
   ```powershell
   docker network create elastic-personalization-network
   ```

4. **Start SQL Server**:
   ```powershell
   docker run -d --name elastic-personalization-db ^
     --network elastic-personalization-network ^
     -e "ACCEPT_EULA=Y" ^
     -e "SA_PASSWORD=YourStrongPassword!" ^
     -e "MSSQL_PID=Express" ^
     -p 1433:1433 ^
     mcr.microsoft.com/mssql/server:2022-latest
   ```

5. **Start Elasticsearch**:
   ```powershell
   docker run -d --name elastic-personalization-es ^
     --network elastic-personalization-network ^
     -e "discovery.type=single-node" ^
     -e "xpack.security.enabled=false" ^
     -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" ^
     -p 9200:9200 ^
     -p 9300:9300 ^
     docker.elastic.co/elasticsearch/elasticsearch:7.17.10
   ```

6. **Start Kibana (optional)**:
   ```powershell
   docker run -d --name elastic-personalization-kibana ^
     --network elastic-personalization-network ^
     -e "ELASTICSEARCH_HOSTS=http://elastic-personalization-es:9200" ^
     -p 5601:5601 ^
     docker.elastic.co/kibana/kibana:7.17.10
   ```

7. **Build and run the API**:
   ```powershell
   # Build the API image
   docker build -t elastic-personalization-api .

   # Run the API container
   docker run -d --name elastic-personalization-api ^
     --network elastic-personalization-network ^
     -e "ASPNETCORE_ENVIRONMENT=Development" ^
     -e "ConnectionStrings__ContentActionsConnection=Server=elastic-personalization-db;Database=ContentActions;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;" ^
     -e "ElasticsearchSettings__Url=http://elastic-personalization-es:9200" ^
     -e "ElasticsearchSettings__DefaultIndex=content" ^
     -p 5000:8080 ^
     elastic-personalization-api
   ```

8. **Verify the containers are running**:
   ```powershell
   docker ps
   ```

9. **Check if the application is healthy**:
   ```powershell
   # Wait a minute for all services to initialize
   # Then check health using curl or browser
   curl http://localhost:5000/api/health
   ```

10. **Access the services**:
    - API: http://localhost:5000
    - Kibana: http://localhost:5601 (useful for exploring Elasticsearch data)

11. **Stopping and cleaning up**:
    ```powershell
    # Stop and remove containers
    docker stop elastic-personalization-api elastic-personalization-es elastic-personalization-db elastic-personalization-kibana
    docker rm elastic-personalization-api elastic-personalization-es elastic-personalization-db elastic-personalization-kibana
    
    # Remove network
    docker network rm elastic-personalization-network
    ```

### Troubleshooting Docker for Windows issues

- **SQL Server container fails to start**:
  ```powershell
  # Check container logs
  docker logs elastic-personalization-db
  
  # Try increasing memory in Docker Desktop settings
  # SQL Server requires at least 2GB RAM
  ```

- **Elasticsearch container fails to start**:
  ```powershell
  # Check container logs
  docker logs elastic-personalization-es
  
  # Elasticsearch may need more memory or increased virtual memory limits
  ```

- **API container fails to connect to database or Elasticsearch**:
  ```powershell
  # Check API container logs
  docker logs elastic-personalization-api
  
  # Verify network connectivity
  docker exec -it elastic-personalization-api ping elastic-personalization-db
  docker exec -it elastic-personalization-api ping elastic-personalization-es
  ```

- **Initialize Elasticsearch index manually**:
  ```powershell
  # If the index wasn't created automatically, initialize it using the health endpoint
  curl -X POST http://localhost:5000/api/health/elasticsearch/initialize
  ```

### Health Monitoring

The application includes a health check endpoint that monitors both the SQL Server database and Elasticsearch:

```
GET /api/health
```

This endpoint returns detailed information about the health of each component and can be used for monitoring.

Additional health endpoints:
```
GET /api/health/database - Check database health
GET /api/health/elasticsearch - Check Elasticsearch health
POST /api/health/elasticsearch/initialize - Create/initialize the Elasticsearch index
POST /api/health/elasticsearch/reindex - Reindex all content in Elasticsearch
```

## 🔧 Project Structure

The project follows a clean architecture pattern with the following components:

- **ElasticPersonalization.Core**: Contains domain entities, interfaces, and business logic
- **ElasticPersonalization.Infrastructure**: Contains implementations of repositories and services
- **ElasticPersonalization.API**: Contains API controllers and configuration

### Key Files and Directories

- `src/ElasticPersonalization.Core/Entities/`: Domain entities for users, content, and interactions
- `src/ElasticPersonalization.Core/Interfaces/`: Service interfaces
- `src/ElasticPersonalization.Infrastructure/Services/`: Service implementations
- `src/ElasticPersonalization.Infrastructure/Data/`: Database context and migrations
- `src/ElasticPersonalization.API/Controllers/`: API endpoints
- `scripts/`: Utility scripts for setup and maintenance
- `docs/`: Documentation assets including diagrams
- `Dockerfile`: API container build instructions

## 📚 Key Features

- Personalized content feed using weighted user interactions
- Full-text search with Elasticsearch
- User interaction tracking (shares, likes, comments)
- User follow relationships
- User preferences and interests
- Transparency in personalization (view personalization factors)
- Health monitoring for system components
- Database migrations and seeding utilities

## 🧪 Testing the API

### Personalization Flow

1. Get personalized feed for user 1:
   ```
   GET /api/personalization/feed/1
   ```

2. See what factors influenced the personalization:
   ```
   GET /api/personalization/factors/1
   ```

3. Add a new interaction (share content):
   ```
   POST /api/userinteraction/share?userId=1&contentId=5
   ```

4. Get the personalized feed again to see how it changed:
   ```
   GET /api/personalization/feed/1
   ```

5. Check the system health:
   ```
   GET /api/health
   ```

## 📦 Development

### Local Development with Docker for Windows

1. Start the required services with Docker (as shown above)
2. Run the API locally (outside Docker):
   ```powershell
   # From the project root
   dotnet run --project src/ElasticPersonalization.API
   ```
3. For debugging in Visual Studio or Visual Studio Code:
   - Open the solution in Visual Studio
   - Update connection strings in appsettings.Development.json to point to containerized services:
   ```json
   {
     "ConnectionStrings": {
       "ContentActionsConnection": "Server=localhost;Database=ContentActions;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;"
     },
     "ElasticsearchSettings": {
       "Url": "http://localhost:9200"
     }
   }
   ```

## 🔄 Recent Updates

The project has been updated to include:

- **Improved database initialization**: Now using Entity Framework migrations properly
- **Enhanced Elasticsearch integration**: Better index management and error handling
- **Dependency handling**: Added wait mechanisms for services to be ready
- **Health checking**: Comprehensive health endpoints with detailed component status
- **Resilience**: Better error handling and retry mechanisms
- **Docker improvements**: Container health checks and dependency management

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.
