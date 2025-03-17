# Elasticsearch Content Personalization POC

A proof of concept in .NET Core 8 that demonstrates how to implement a personalized content feed based on user interactions with Elasticsearch and a relational database for content actions.

## Overview

This proof of concept demonstrates how to create a personalized content feed by leveraging:

- **Elasticsearch** for efficient content search and scoring
- **SQL Server** for storing user interactions
- **Configurable weights** for different user actions

The system personalizes content based on a hierarchy of user interactions:

1. **Share actions** (highest weight)
2. **Comment actions** 
3. **Like actions**
4. **Follow relationships**
5. **User preferences**
6. **User interests** (lowest weight)

<div class="arch-diagram-container">
  <img src="https://raw.githubusercontent.com/glglak/elastic-personalization-poc/main/docs/architecture.svg" alt="Architecture Diagram" />
</div>

## Architecture

The architecture follows a clean, layered approach:

- **API Layer**: RESTful endpoints for content, personalization, and user interactions
- **Core Layer**: Domain entities, interfaces, and business models
- **Infrastructure Layer**: Service implementations and data access
- **Data Stores**: SQL Server for user interactions and Elasticsearch for content indexing

## Personalization Flow

<div class="arch-diagram-container">
  <img src="https://raw.githubusercontent.com/glglak/elastic-personalization-poc/main/docs/personalization-flow.svg" alt="Personalization Flow" />
</div>

The personalization algorithm works by:

1. **User Profile Collection** - Gathering explicit preferences and interests from the user profile
2. **User Interactions** - Analyzing implicit signals from user behavior (shares, likes, comments, follows)
3. **Weight Calculation** - Applying configurable weights to each interaction type
4. **Query Generation** - Building an Elasticsearch query with function scoring
5. **Personalized Feed** - Returning scored and ranked content

## Key Features

<ul class="feature-list">
  <li>Personalized content feed using weighted user interactions</li>
  <li>Full-text search with Elasticsearch</li>
  <li>User interaction tracking (shares, likes, comments)</li>
  <li>User follow relationships</li>
  <li>User preferences and interests</li>
  <li>Transparency in personalization (view personalization factors)</li>
</ul>

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

## API Endpoints

### Content API

<div class="api-endpoint">
  <code>GET /api/content/{id}</code> - Get content by ID
</div>

<div class="api-endpoint">
  <code>POST /api/content</code> - Create new content
</div>

<div class="api-endpoint">
  <code>PUT /api/content/{id}</code> - Update content
</div>

<div class="api-endpoint">
  <code>DELETE /api/content/{id}</code> - Delete content
</div>

<div class="api-endpoint">
  <code>GET /api/content/search?query={query}</code> - Search content
</div>

<div class="api-endpoint">
  <code>GET /api/content/category/{category}</code> - Get content by category
</div>

<div class="api-endpoint">
  <code>GET /api/content/tag/{tag}</code> - Get content by tag
</div>

<div class="api-endpoint">
  <code>GET /api/content/creator/{creatorId}</code> - Get content by creator
</div>

### Personalization API

<div class="api-endpoint">
  <code>GET /api/personalization/feed/{userId}</code> - Get personalized feed for user
</div>

<div class="api-endpoint">
  <code>GET /api/personalization/score/{userId}/{contentId}</code> - Calculate personalization score
</div>

<div class="api-endpoint">
  <code>GET /api/personalization/factors/{userId}</code> - Get personalization factors for transparency
</div>

### User Interaction API

<div class="api-endpoint">
  <code>POST /api/userinteraction/share?userId={userId}&contentId={contentId}</code> - Share content
</div>

<div class="api-endpoint">
  <code>DELETE /api/userinteraction/share?userId={userId}&contentId={contentId}</code> - Remove share
</div>

<div class="api-endpoint">
  <code>POST /api/userinteraction/like?userId={userId}&contentId={contentId}</code> - Like content
</div>

<div class="api-endpoint">
  <code>DELETE /api/userinteraction/like?userId={userId}&contentId={contentId}</code> - Remove like
</div>

<div class="api-endpoint">
  <code>POST /api/userinteraction/comment?userId={userId}&contentId={contentId}</code> - Comment on content
</div>

<div class="api-endpoint">
  <code>DELETE /api/userinteraction/comment/{commentId}</code> - Remove comment
</div>

<div class="api-endpoint">
  <code>POST /api/userinteraction/follow?userId={userId}&followedUserId={followedUserId}</code> - Follow user
</div>

<div class="api-endpoint">
  <code>DELETE /api/userinteraction/follow?userId={userId}&followedUserId={followedUserId}</code> - Unfollow user
</div>

## Personalization Configuration

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

<table>
  <thead>
    <tr>
      <th>Interaction Type</th>
      <th>Weight</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Share</td>
      <td>5.0</td>
      <td>Highest weight - strong signal of user interest</td>
    </tr>
    <tr>
      <td>Comment</td>
      <td>4.0</td>
      <td>High engagement, indicates significant interest</td>
    </tr>
    <tr>
      <td>Follow</td>
      <td>4.5</td>
      <td>Strong connection to content creator</td>
    </tr>
    <tr>
      <td>Like</td>
      <td>3.0</td>
      <td>Moderate interest signal</td>
    </tr>
    <tr>
      <td>Preference</td>
      <td>2.0</td>
      <td>Explicit user preference (category)</td>
    </tr>
    <tr>
      <td>Interest</td>
      <td>1.5</td>
      <td>Explicit user interest (tag)</td>
    </tr>
  </tbody>
</table>

## Project Structure

The project follows a clean architecture pattern with the following components:

- **ElasticPersonalization.Core**: Contains domain entities, interfaces, and business logic
- **ElasticPersonalization.Infrastructure**: Contains implementations of repositories and services
- **ElasticPersonalization.API**: Contains API controllers and configuration

For more detailed information, please check the [GitHub repository](https://github.com/glglak/elastic-personalization-poc).
