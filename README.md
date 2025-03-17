# Elasticsearch Content Personalization POC

A proof of concept in .NET Core 8 that demonstrates how to implement a personalized content feed based on user interactions with Elasticsearch and a relational database for content actions.

## Overview

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

## Project Structure

The project follows a clean architecture pattern with the following components:

- **ElasticPersonalization.Core**: Contains domain entities, interfaces, and business logic
- **ElasticPersonalization.Infrastructure**: Contains implementations of repositories and services
- **ElasticPersonalization.API**: Contains API controllers and configuration

## Key Features

- Personalized content feed using weighted user interactions
- Full-text search with Elasticsearch
- User interaction tracking (shares, likes, comments)
- User follow relationships
- User preferences and interests
- Transparency in personalization (view personalization factors)

## Prerequisites

- .NET 8 SDK
- Docker and Docker Compose

## Getting Started

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

### Manual Setup

If you prefer to set up manually:

1. Start the services:
   ```
   docker-compose up -d
   ```

2. Initialize SQL Server:
   ```
   docker-compose exec -T db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrongPassword! -i /var/opt/mssql/scripts/InitializeDB.sql
   ```

3. Initialize Elasticsearch:
   ```
   docker-compose exec -T app node /app/scripts/InitializeElasticsearch.js http://elasticsearch:9200 content
   ```

## Development Setup

For development:

1. Install .NET 8 SDK
2. Start SQL Server and Elasticsearch using Docker:
   ```
   docker-compose up -d db elasticsearch kibana
   ```
3. Update connection strings in `src/ElasticPersonalization.API/appsettings.Development.json`
4. Run the application:
   ```
   dotnet run --project src/ElasticPersonalization.API
   ```

## API Endpoints

### Content API

- `GET /api/content/{id}` - Get content by ID
- `POST /api/content` - Create new content
- `PUT /api/content/{id}` - Update content
- `DELETE /api/content/{id}` - Delete content
- `GET /api/content/search?query={query}` - Search content
- `GET /api/content/category/{category}` - Get content by category
- `GET /api/content/tag/{tag}` - Get content by tag
- `GET /api/content/creator/{creatorId}` - Get content by creator

### Personalization API

- `GET /api/personalization/feed/{userId}` - Get personalized feed for user
- `GET /api/personalization/score/{userId}/{contentId}` - Calculate personalization score
- `GET /api/personalization/factors/{userId}` - Get personalization factors for transparency

### User Interaction API

- `POST /api/userinteraction/share?userId={userId}&contentId={contentId}` - Share content
- `DELETE /api/userinteraction/share?userId={userId}&contentId={contentId}` - Remove share
- `POST /api/userinteraction/like?userId={userId}&contentId={contentId}` - Like content
- `DELETE /api/userinteraction/like?userId={userId}&contentId={contentId}` - Remove like
- `POST /api/userinteraction/comment?userId={userId}&contentId={contentId}` - Comment on content
- `DELETE /api/userinteraction/comment/{commentId}` - Remove comment
- `POST /api/userinteraction/follow?userId={userId}&followedUserId={followedUserId}` - Follow user
- `DELETE /api/userinteraction/follow?userId={userId}&followedUserId={followedUserId}` - Unfollow user
- `POST /api/userinteraction/preference?userId={userId}` - Add user preference
- `DELETE /api/userinteraction/preference?userId={userId}&preference={preference}` - Remove user preference
- `POST /api/userinteraction/interest?userId={userId}` - Add user interest
- `DELETE /api/userinteraction/interest?userId={userId}&interest={interest}` - Remove user interest

## Testing the API

You can use the included sample data to test the API:

1. Get personalized feed for user 1:
   ```
   GET /api/personalization/feed/1
   ```

2. See what factors influenced the personalization:
   ```
   GET /api/personalization/factors/1
   ```

3. Share content:
   ```
   POST /api/userinteraction/share?userId=1&contentId=5
   ```

4. Get the personalized feed again to see how it changed:
   ```
   GET /api/personalization/feed/1
   ```

## Personalization Algorithm

The personalization algorithm works by:

1. Collecting user interaction data (shares, likes, comments)
2. Analyzing user follow relationships
3. Considering explicit user preferences and interests
4. Weighting these factors according to configuration
5. Using Elasticsearch function scoring to boost relevant content

The weights for each factor can be configured in `appsettings.json`:

```json
"PersonalizationWeights": {
  "ShareWeight": 5.0,
  "CommentWeight": 4.0,
  "LikeWeight": 3.0,
  "FollowWeight": 4.5,
  "PreferenceWeight": 2.0,
  "InterestWeight": 1.5
}
```

## Future Enhancements

- A/B testing for personalization algorithms
- Machine learning for weight optimization
- Collaborative filtering
- Content-based recommendations
- Advanced analytics on user engagement

## License

This project is licensed under the MIT License - see the LICENSE file for details.
