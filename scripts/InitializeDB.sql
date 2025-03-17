-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'ContentActions')
BEGIN
    CREATE DATABASE ContentActions;
END
GO

USE ContentActions;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL
    );
END
GO

-- Create Content table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Content')
BEGIN
    CREATE TABLE Content (
        Id NVARCHAR(36) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        Body NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CreatorId INT NOT NULL,
        FOREIGN KEY (CreatorId) REFERENCES Users(Id)
    );
END
GO

-- Create Shares table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Shares')
BEGIN
    CREATE TABLE Shares (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        ContentId NVARCHAR(36) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (ContentId) REFERENCES Content(Id)
    );
END
GO

-- Create Likes table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Likes')
BEGIN
    CREATE TABLE Likes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        ContentId NVARCHAR(36) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (ContentId) REFERENCES Content(Id)
    );
END
GO

-- Create Comments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments')
BEGIN
    CREATE TABLE Comments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        ContentId NVARCHAR(36) NOT NULL,
        CommentText NVARCHAR(1000) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (ContentId) REFERENCES Content(Id)
    );
END
GO

-- Create Follows table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Follows')
BEGIN
    CREATE TABLE Follows (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        FollowedUserId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (FollowedUserId) REFERENCES Users(Id),
        CONSTRAINT UC_Follow UNIQUE (UserId, FollowedUserId)
    );
END
GO

-- Create table for content tags
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContentTags')
BEGIN
    CREATE TABLE ContentTags (
        ContentId NVARCHAR(36) NOT NULL,
        Tag NVARCHAR(50) NOT NULL,
        FOREIGN KEY (ContentId) REFERENCES Content(Id),
        CONSTRAINT PK_ContentTags PRIMARY KEY (ContentId, Tag)
    );
END
GO

-- Create table for content categories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContentCategories')
BEGIN
    CREATE TABLE ContentCategories (
        ContentId NVARCHAR(36) NOT NULL,
        Category NVARCHAR(50) NOT NULL,
        FOREIGN KEY (ContentId) REFERENCES Content(Id),
        CONSTRAINT PK_ContentCategories PRIMARY KEY (ContentId, Category)
    );
END
GO

-- Create table for user preferences
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPreferences')
BEGIN
    CREATE TABLE UserPreferences (
        UserId INT NOT NULL,
        Preference NVARCHAR(50) NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT PK_UserPreferences PRIMARY KEY (UserId, Preference)
    );
END
GO

-- Create table for user interests
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserInterests')
BEGIN
    CREATE TABLE UserInterests (
        UserId INT NOT NULL,
        Interest NVARCHAR(50) NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT PK_UserInterests PRIMARY KEY (UserId, Interest)
    );
END
GO

-- Add indexes for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shares_UserId')
    CREATE INDEX IX_Shares_UserId ON Shares(UserId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shares_ContentId')
    CREATE INDEX IX_Shares_ContentId ON Shares(ContentId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Likes_UserId')
    CREATE INDEX IX_Likes_UserId ON Likes(UserId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Likes_ContentId')
    CREATE INDEX IX_Likes_ContentId ON Likes(ContentId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_UserId')
    CREATE INDEX IX_Comments_UserId ON Comments(UserId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_ContentId')
    CREATE INDEX IX_Comments_ContentId ON Comments(ContentId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Follows_UserId')
    CREATE INDEX IX_Follows_UserId ON Follows(UserId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Follows_FollowedUserId')
    CREATE INDEX IX_Follows_FollowedUserId ON Follows(FollowedUserId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Content_CreatorId')
    CREATE INDEX IX_Content_CreatorId ON Content(CreatorId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ContentTags_Tag')
    CREATE INDEX IX_ContentTags_Tag ON ContentTags(Tag);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ContentCategories_Category')
    CREATE INDEX IX_ContentCategories_Category ON ContentCategories(Category);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserPreferences_Preference')
    CREATE INDEX IX_UserPreferences_Preference ON UserPreferences(Preference);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserInterests_Interest')
    CREATE INDEX IX_UserInterests_Interest ON UserInterests(Interest);
GO

-- Insert sample data
-- Users
IF NOT EXISTS (SELECT TOP 1 * FROM Users)
BEGIN
    INSERT INTO Users (Username, Email) VALUES
    ('user1', 'user1@example.com'),
    ('user2', 'user2@example.com'),
    ('user3', 'user3@example.com'),
    ('user4', 'user4@example.com'),
    ('user5', 'user5@example.com');

    -- Sample content
    INSERT INTO Content (Id, Title, Description, Body, CreatedAt, CreatorId) VALUES
    ('1', 'Introduction to Elasticsearch', 'Learn the basics of Elasticsearch', 'Elasticsearch is a distributed, RESTful search and analytics engine...', GETDATE(), 1),
    ('2', 'Advanced .NET Core Development', 'Take your .NET skills to the next level', 'In this article, we will explore advanced concepts in .NET Core...', GETDATE(), 2),
    ('3', 'Building Personalized Content Feeds', 'Learn how to create personalized experiences', 'Personalization is key to user engagement...', GETDATE(), 1),
    ('4', 'SQL Server Performance Tuning', 'Optimize your database queries', 'This guide covers essential techniques for SQL Server optimization...', GETDATE(), 3),
    ('5', 'Getting Started with Docker', 'Containerize your applications', 'Docker is a platform for developing, shipping, and running applications...', GETDATE(), 2);

    -- Content Tags
    INSERT INTO ContentTags (ContentId, Tag) VALUES
    ('1', 'elasticsearch'),
    ('1', 'search'),
    ('1', 'database'),
    ('2', 'dotnet'),
    ('2', 'csharp'),
    ('2', 'programming'),
    ('3', 'personalization'),
    ('3', 'recommendation'),
    ('3', 'user-experience'),
    ('4', 'sql'),
    ('4', 'database'),
    ('4', 'performance'),
    ('5', 'docker'),
    ('5', 'containerization'),
    ('5', 'devops');

    -- Content Categories
    INSERT INTO ContentCategories (ContentId, Category) VALUES
    ('1', 'Database'),
    ('1', 'Search'),
    ('2', 'Programming'),
    ('2', 'Web Development'),
    ('3', 'User Experience'),
    ('3', 'Personalization'),
    ('4', 'Database'),
    ('4', 'Performance'),
    ('5', 'DevOps'),
    ('5', 'Containers');

    -- User Preferences
    INSERT INTO UserPreferences (UserId, Preference) VALUES
    (1, 'Programming'),
    (1, 'Database'),
    (2, 'DevOps'),
    (2, 'Web Development'),
    (3, 'Database'),
    (3, 'Performance'),
    (4, 'Search'),
    (4, 'User Experience'),
    (5, 'DevOps'),
    (5, 'Containers');

    -- User Interests
    INSERT INTO UserInterests (UserId, Interest) VALUES
    (1, 'elasticsearch'),
    (1, 'dotnet'),
    (2, 'docker'),
    (2, 'programming'),
    (3, 'sql'),
    (3, 'database'),
    (4, 'search'),
    (4, 'user-experience'),
    (5, 'containerization'),
    (5, 'devops');

    -- Sample Follows
    INSERT INTO Follows (UserId, FollowedUserId, CreatedAt) VALUES
    (1, 2, GETDATE()),
    (1, 3, GETDATE()),
    (2, 1, GETDATE()),
    (3, 1, GETDATE()),
    (4, 1, GETDATE()),
    (4, 2, GETDATE()),
    (5, 2, GETDATE());

    -- Sample Likes
    INSERT INTO Likes (UserId, ContentId, CreatedAt) VALUES
    (1, '2', GETDATE()),
    (1, '4', GETDATE()),
    (2, '1', GETDATE()),
    (2, '3', GETDATE()),
    (3, '1', GETDATE()),
    (3, '5', GETDATE()),
    (4, '3', GETDATE()),
    (5, '2', GETDATE()),
    (5, '5', GETDATE());

    -- Sample Comments
    INSERT INTO Comments (UserId, ContentId, CommentText, CreatedAt) VALUES
    (1, '2', 'Great article on .NET Core!', GETDATE()),
    (2, '1', 'Very helpful introduction to Elasticsearch', GETDATE()),
    (3, '4', 'These optimization tips worked well for me', GETDATE()),
    (4, '3', 'Love the personalization insights', GETDATE()),
    (5, '5', 'Docker is changing how we deploy applications', GETDATE());

    -- Sample Shares
    INSERT INTO Shares (UserId, ContentId, CreatedAt) VALUES
    (1, '3', GETDATE()),
    (2, '2', GETDATE()),
    (3, '1', GETDATE()),
    (4, '5', GETDATE()),
    (5, '4', GETDATE());
END
GO

PRINT 'Database initialization completed successfully!';
