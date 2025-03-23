FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only project files first to take advantage of Docker's layer caching
COPY ["ElasticPersonalization.sln", "."]
COPY ["src/ElasticPersonalization.API/ElasticPersonalization.API.csproj", "src/ElasticPersonalization.API/"]
COPY ["src/ElasticPersonalization.Core/ElasticPersonalization.Core.csproj", "src/ElasticPersonalization.Core/"]
COPY ["src/ElasticPersonalization.Infrastructure/ElasticPersonalization.Infrastructure.csproj", "src/ElasticPersonalization.Infrastructure/"]

# Install Node.js for Elasticsearch initialization script
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Restore NuGet packages
RUN dotnet restore "ElasticPersonalization.sln"

# Copy all source code
COPY . .

# Build the projects
RUN dotnet build "src/ElasticPersonalization.Core/ElasticPersonalization.Core.csproj" -c Release -o /app/build
RUN dotnet build "src/ElasticPersonalization.Infrastructure/ElasticPersonalization.Infrastructure.csproj" -c Release -o /app/build
RUN dotnet build "src/ElasticPersonalization.API/ElasticPersonalization.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/ElasticPersonalization.API/ElasticPersonalization.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Copy initialization scripts
COPY scripts/InitializeElasticsearch.js /app/publish/scripts/
COPY scripts/wait-for-it.sh /app/publish/scripts/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install Node.js in the final container for Elasticsearch initialization
RUN apt-get update && \
    apt-get install -y curl netcat-openbsd && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Make scripts executable
RUN chmod +x /app/scripts/wait-for-it.sh

# Create appsettings files with default values
RUN echo '{"Logging":{"LogLevel":{"Default":"Information","Microsoft.AspNetCore":"Warning"}},"AllowedHosts":"*","ConnectionStrings":{"ContentActionsConnection":"Server=db;Database=ContentActions;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;"},"ElasticsearchSettings":{"Url":"http://elasticsearch:9200","DefaultIndex":"content","Username":"elastic","Password":"changeme"},"PersonalizationWeights":{"ShareWeight":5.0,"CommentWeight":4.0,"LikeWeight":3.0,"FollowWeight":4.5,"PreferenceWeight":2.0,"InterestWeight":1.5}}' > appsettings.json
RUN echo '{"Logging":{"LogLevel":{"Default":"Information","Microsoft.AspNetCore":"Warning"}}}' > appsettings.Development.json

# Configure healthcheck
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/api/health || exit 1

# Configure ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080

# We'll use a shell script as entrypoint to wait for dependencies 
# and then run the application
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

ENTRYPOINT ["/app/docker-entrypoint.sh"]
