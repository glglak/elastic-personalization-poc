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

### Using Docker for Windows

1. **Prerequisites**:
   - [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop/) installed and running
   - [Git for Windows](https://git-scm.com/download/win) for cloning the repository
   - WSL 2 enabled (recommended for better performance)

2. **Clone the repository**:
   ```powershell
   git clone https://github.com/glglak/elastic-personalization-poc.git
   cd elastic-personalization-poc
   ```

3. **Start the services with Docker Compose**:
   ```powershell
   # Make sure Docker Desktop is running first
   docker-compose up -d
   ```

   This will:
   - Build the API image
   - Start SQL Server
   - Start Elasticsearch and Kibana
   - Initialize the database with migrations
   - Set up Elasticsearch indices

4. **Verify the services are running**:
   ```powershell
   docker-compose ps
   ```

   All services should show as "running" and health checks should pass after a minute or two.

5. **Check application health**:
   ```powershell
   # Using curl (if installed)
   curl http://localhost:5000/api/health

   # Or simply open in your browser
   # http://localhost:5000/api/health
   ```

6. **Access the services**:
   - API: http://localhost:5000
   - Kibana: http://localhost:5601 (useful for exploring Elasticsearch data)

7. **Troubleshooting Docker for Windows issues**:
   - If containers fail to start, check Docker Desktop logs
   - Ensure WSL 2 is enabled for better performance
   - Try increasing Docker Desktop resource allocations (Memory, CPU)
   - For port conflicts, modify the port mappings in docker-compose.yml

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
- `docker-compose.yml`: Docker services configuration
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

1. Clone the repository
2. Start the required services:
   ```powershell
   # Start only database and Elasticsearch
   docker-compose up -d db elasticsearch kibana
   ```
3. Run the API locally (outside Docker):
   ```powershell
   # From the project root
   dotnet run --project src/ElasticPersonalization.API
   ```
4. For debugging in Visual Studio or Visual Studio Code:
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

### Stopping the Services

```powershell
# Stop all containers but preserve data
docker-compose down

# Or to completely remove everything including volumes
docker-compose down -v
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
