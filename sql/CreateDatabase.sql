-- PresalesDemo database: run on localhost\SQLEXPRESS. Idempotent table creation.
-- The app also calls EnsureCreated() on startup, so this script is an alternative / audit copy.
-- Do NOT run this against a real OneNow or TaskApi database. Database name is PresalesDemo only.
USE [PresalesDemo];
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Profiles')
CREATE TABLE Profiles (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(200) NOT NULL, Industry NVARCHAR(50) NOT NULL, Locations INT NOT NULL, EmployeeCount INT NOT NULL, IsDefault BIT NOT NULL, IsDemoSample BIT NOT NULL, CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NOT NULL);
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DiscoveryItems')
CREATE TABLE DiscoveryItems (Id INT IDENTITY(1,1) PRIMARY KEY, ProfileId INT NOT NULL, [Group] NVARCHAR(100) NOT NULL, Question NVARCHAR(500) NOT NULL, Answer NVARCHAR(MAX) NOT NULL, SortOrder INT NOT NULL);
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Priorities')
CREATE TABLE Priorities (Id INT IDENTITY(1,1) PRIMARY KEY, ProfileId INT NOT NULL, Rank INT NOT NULL, Title NVARCHAR(500) NOT NULL, Description NVARCHAR(MAX) NOT NULL);
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Scenarios')
CREATE TABLE Scenarios (Id INT IDENTITY(1,1) PRIMARY KEY, ProfileId INT NOT NULL, SortOrder INT NOT NULL, Title NVARCHAR(500) NOT NULL, Pain NVARCHAR(MAX) NOT NULL, Solution NVARCHAR(MAX) NOT NULL, Outcome NVARCHAR(MAX) NOT NULL, Value NVARCHAR(MAX) NOT NULL, ScreenFlow NVARCHAR(MAX) NOT NULL, StorySteps NVARCHAR(MAX) NOT NULL, BeforeText NVARCHAR(MAX) NOT NULL, AfterText NVARCHAR(MAX) NOT NULL);
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FitGapRows')
CREATE TABLE FitGapRows (Id INT IDENTITY(1,1) PRIMARY KEY, ProfileId INT NOT NULL, Requirement NVARCHAR(500) NOT NULL, Status NVARCHAR(50) NOT NULL, Note NVARCHAR(MAX) NOT NULL, SortOrder INT NOT NULL);
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Scorecards')
CREATE TABLE Scorecards (Id INT IDENTITY(1,1) PRIMARY KEY, ProfileId INT NOT NULL UNIQUE, UnderstandCustomer INT NOT NULL, RightProblem INT NOT NULL, BusinessValue INT NOT NULL, HandleObjections INT NOT NULL, NextStep INT NOT NULL);
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Actions')
CREATE TABLE Actions (Id INT IDENTITY(1,1) PRIMARY KEY, ProfileId INT NOT NULL, Title NVARCHAR(500) NOT NULL, Owner NVARCHAR(200) NOT NULL, Deadline NVARCHAR(100) NOT NULL, Source NVARCHAR(100) NOT NULL, IsDone BIT NOT NULL DEFAULT 0);
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FollowUps')
CREATE TABLE FollowUps (Id INT IDENTITY(1,1) PRIMARY KEY, ProfileId INT NOT NULL, Title NVARCHAR(500) NOT NULL, Owner NVARCHAR(200) NOT NULL, Deadline NVARCHAR(100) NOT NULL, SourceFitGap NVARCHAR(500) NOT NULL);
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChecklistItems')
CREATE TABLE ChecklistItems (Id INT IDENTITY(1,1) PRIMARY KEY, ProfileId INT NOT NULL, [Group] NVARCHAR(100) NOT NULL, Title NVARCHAR(500) NOT NULL, IsDone BIT NOT NULL, IsOneNowMandatory BIT NOT NULL, SortOrder INT NOT NULL);
GO
