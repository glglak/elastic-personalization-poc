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

## Architecture

![Architecture Diagram](https://raw.githubusercontent.com/glglak/elastic-personalization-poc/main/docs/architecture.svg)

The architecture follows a clean, layered approach:

- **API Layer**: RESTful endpoints for content, personalization, and user interactions
- **Core Layer**: Domain entities, interfaces, and business models
- **Infrastructure Layer**: Service implementations and data access
- **Data Stores**: SQL Server for user interactions and Elasticsearch for content indexing

## Personalization Flow

![Personalization Flow](https://raw.githubusercontent.com/glglak/elastic-personalization-poc/main/docs/personalization-flow.svg)

The personalization algorithm works by:

1. **User Profile Collection** - Gathering explicit preferences and interests from the user profile
2. **User Interactions** - Analyzing implicit signals from user behavior (shares, likes, comments, follows)
3. **Weight Calculation** - Applying configurable weights to each interaction type
4. **Query Generation** - Building an Elasticsearch query with function scoring
5. **Personalized Feed** - Returning scored and ranked content

## Key Features

- Personalized content feed using weighted user interactions
- Full-text search with Elasticsearch
- User interaction tracking (shares, likes, comments)
- User follow relationships
- User preferences and interests
- Transparency in personalization (view personalization factors)

## Getting Started

### With Docker

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

### API Endpoints

#### Content API

- `GET /api/content/{id}` - Get content by ID
- `POST /api/content` - Create new content
- `PUT /api/content/{id}` - Update content
- `DELETE /api/content/{id}` - Delete content
- `GET /api/content/search?query={query}` - Search content
- `GET /api/content/category/{category}` - Get content by category
- `GET /api/content/tag/{tag}` - Get content by tag
- `GET /api/content/creator/{creatorId}` - Get content by creator

#### Personalization API

- `GET /api/personalization/feed/{userId}` - Get personalized feed for user
- `GET /api/personalization/score/{userId}/{contentId}` - Calculate personalization score
- `GET /api/personalization/factors/{userId}` - Get personalization factors for transparency

#### User Interaction API

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

### Personalization Configuration

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

## Project Structure

The project follows a clean architecture pattern with the following components:

- **ElasticPersonalization.Core**: Contains domain entities, interfaces, and business logic
- **ElasticPersonalization.Infrastructure**: Contains implementations of repositories and services
- **ElasticPersonalization.API**: Contains API controllers and configuration

For more detailed information, please check the [GitHub repository](https://github.com/glglak/elastic-personalization-poc).
