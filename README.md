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
- SQL Server
- Elasticsearch 7.x
- Docker (optional, for containerization)

## Getting Started

1. Clone the repository
2. Update connection strings in `appsettings.json` to point to your SQL Server and Elasticsearch instances
3. Run database migrations: `dotnet ef database update --project src/ElasticPersonalization.Infrastructure --startup-project src/ElasticPersonalization.API`
4. Start the application: `dotnet run --project src/ElasticPersonalization.API`

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

## Database Schema

The database schema includes:

- Users with preferences and interests
- Content items with tags and categories
- User interactions (shares, likes, comments)
- Follow relationships between users

## Elasticsearch Mapping

Content is indexed in Elasticsearch with the following structure:

- ID
- Title (boosted field for search)
- Description
- Body
- Tags (for interest matching)
- Categories (for preference matching)
- Creator ID (for follow relationships)
- Created date (for recency boosting)

## Future Enhancements

- A/B testing for personalization algorithms
- Machine learning for weight optimization
- Collaborative filtering
- Content-based recommendations
- Advanced analytics on user engagement
