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

The easiest way to get started is using Docker Compose:

```bash
# Clone the repository
git clone https://github.com/glglak/elastic-personalization-poc.git
cd elastic-personalization-poc

# Start the services and initialize the environment
chmod +x scripts/setup.sh
./scripts/setup.sh
```

The script will:
1. Start SQL Server, Elasticsearch, Kibana, and the API
2. Initialize the SQL Server database with sample data
3. Create and populate the Elasticsearch index

Once complete, you can access:
- API: http://localhost:5000
- Kibana: http://localhost:5601

### Health Monitoring

The application includes a health check endpoint that monitors both the SQL Server database and Elasticsearch:

```
GET /api/health
```

This endpoint returns detailed information about the health of each component and can be used for monitoring.

### Manual Database Setup

If you need to manually set up or refresh the database, use the provided scripts:

**Windows (PowerShell):**
```powershell
./scripts/RunMigrations.ps1
```

**Linux/macOS:**
```bash
chmod +x scripts/RunMigrations.sh
./scripts/RunMigrations.sh
```

These scripts will:
1. Apply all Entity Framework migrations to create the database schema
2. Seed the database with sample data
3. Create and populate the Elasticsearch index

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

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose (for local development with containers)
- Entity Framework Core tools: `dotnet tool install --global dotnet-ef`

### Local Development

1. Clone the repository
2. Start the required services (SQL Server, Elasticsearch):
   ```
   docker-compose up -d db elasticsearch kibana
   ```
3. Run database migrations:
   ```
   # Windows
   ./scripts/RunMigrations.ps1
   
   # Linux/macOS
   ./scripts/RunMigrations.sh
   ```
4. Start the API:
   ```
   dotnet run --project src/ElasticPersonalization.API
   ```

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.
