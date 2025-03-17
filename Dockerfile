FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ElasticPersonalization.sln", "./"]
COPY ["src/ElasticPersonalization.API/ElasticPersonalization.API.csproj", "src/ElasticPersonalization.API/"]
COPY ["src/ElasticPersonalization.Core/ElasticPersonalization.Core.csproj", "src/ElasticPersonalization.Core/"]
COPY ["src/ElasticPersonalization.Infrastructure/ElasticPersonalization.Infrastructure.csproj", "src/ElasticPersonalization.Infrastructure/"]
RUN dotnet restore
COPY . .
WORKDIR "/src"
RUN dotnet build "ElasticPersonalization.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/ElasticPersonalization.API/ElasticPersonalization.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ElasticPersonalization.API.dll"]
