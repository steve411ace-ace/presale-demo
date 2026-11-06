-- PresalesDemo database: run on localhost\SQLEXPRESS. Idempotent table creation.
-- The app also calls EnsureCreated() on startup, so this script is an alternative / audit copy.
-- Do NOT run this against a real OneNow or TaskApi database. Database name is PresalesDemo only.
USE [PresalesDemo];
GO
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Profiles')
CREATE TABLE Profiles (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(200) NOT NULL, Industry NVARCHAR(50) NOT NULL, Locations INT NOT NULL, EmployeeCount INT NOT NULL, IsDefault BIT NOT NULL, IsDemoSample BIT NOT NULL, AiSuggestionRequested BIT NOT NULL DEFAULT 0, AiSuggestionRequestedAt DATETIME2 NULL, CompanyResearchRequested BIT NOT NULL DEFAULT 0, CompanyResearchRequestedAt DATETIME2 NULL, CompanyResearchNote NVARCHAR(MAX) NOT NULL DEFAULT '', CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NOT NULL);
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

-- Pre-demo survey: additive tables, no changes to existing profiles.
IF OBJECT_ID(N'dbo.SurveyQuestions', N'U') IS NULL
CREATE TABLE [dbo].[SurveyQuestions] (
[Id] int IDENTITY(1,1) PRIMARY KEY,
[SortOrder] int NOT NULL,
[Title] nvarchar(max) NOT NULL,
[Hint] nvarchar(max) NOT NULL,
[Type] nvarchar(max) NOT NULL,
[OptionsJson] nvarchar(max) NOT NULL,
[FieldsJson] nvarchar(max) NOT NULL,
[Placeholder] nvarchar(max) NOT NULL,
[MinValue] int NULL,
[MaxValue] int NULL,
[ScaleMax] int NULL,
[Required] bit NOT NULL,
[IsActive] bit NOT NULL,
[MediaUrl] nvarchar(max) NULL,
[MediaType] nvarchar(max) NULL
);
IF OBJECT_ID(N'dbo.SurveyResponses', N'U') IS NULL
CREATE TABLE [dbo].[SurveyResponses] (
[Id] int IDENTITY(1,1) PRIMARY KEY,
[SubmittedAt] datetime2 NOT NULL,
[ContactName] nvarchar(max) NOT NULL,
[ContactEmail] nvarchar(max) NOT NULL,
[Role] nvarchar(max) NOT NULL,
[CompanyName] nvarchar(max) NOT NULL,
[Industry] nvarchar(max) NOT NULL,
[EmployeeBucket] nvarchar(max) NOT NULL,
[LocationsBucket] nvarchar(max) NOT NULL,
[CurrentSystem] nvarchar(max) NOT NULL,
[PainPointsJson] nvarchar(max) NOT NULL,
[ShiftType] nvarchar(max) NOT NULL,
[DecisionMaker] nvarchar(max) NOT NULL,
[ApplyTimeline] nvarchar(max) NOT NULL,
[DemoMustSee] nvarchar(max) NOT NULL,
[AnswersJson] nvarchar(max) NOT NULL,
[QuestionsJson] nvarchar(max) NOT NULL,
[ProfileId] int NULL
);
GO

IF OBJECT_ID(N'dbo.SurveyPresentations', N'U') IS NULL
CREATE TABLE [dbo].[SurveyPresentations] (
[Id] int IDENTITY(1,1) PRIMARY KEY,
[MediaUrl] nvarchar(max) NULL,
[MediaType] nvarchar(max) NULL
);
GO

-- CRM: primary schema source. Program.cs addCols migrates profile metadata at startup.
IF COL_LENGTH('Profiles','Status') IS NULL ALTER TABLE Profiles ADD Status nvarchar(20) NOT NULL DEFAULT N'DangChuanBi';
IF COL_LENGTH('Profiles','DemoDate') IS NULL ALTER TABLE Profiles ADD DemoDate datetime2 NULL;
IF COL_LENGTH('Profiles','Owner') IS NULL ALTER TABLE Profiles ADD Owner nvarchar(100) NULL;
IF COL_LENGTH('Profiles','FollowUpNote') IS NULL ALTER TABLE Profiles ADD FollowUpNote nvarchar(1000) NULL;
-- BEGIN CRM TABLES
IF OBJECT_ID(N'Interactions', N'U') IS NULL
BEGIN
CREATE TABLE Interactions (
 Id int IDENTITY PRIMARY KEY, ProfileId int NOT NULL REFERENCES Profiles(Id) ON DELETE CASCADE,
 Type nvarchar(20) NOT NULL, OccurredAt datetime2 NOT NULL, Note nvarchar(2000) NOT NULL,
 CreatedBy nvarchar(100) NULL, CreatedAt datetime2 NOT NULL
);
CREATE INDEX IX_Interactions_ProfileId ON Interactions(ProfileId);
END;
IF OBJECT_ID(N'DemoAppointments', N'U') IS NULL
BEGIN
CREATE TABLE DemoAppointments (
 Id int IDENTITY PRIMARY KEY, ProfileId int NOT NULL REFERENCES Profiles(Id) ON DELETE CASCADE,
 Title nvarchar(200) NOT NULL, StartsAt datetime2 NOT NULL, EndsAt datetime2 NULL,
 Location nvarchar(1000) NULL, Notes nvarchar(2000) NULL, ReminderMinutes int NULL DEFAULT 60,
 Status nvarchar(20) NOT NULL, CreatedAt datetime2 NOT NULL
);
CREATE INDEX IX_DemoAppointments_ProfileId ON DemoAppointments(ProfileId);
END;
IF OBJECT_ID(N'Attachments', N'U') IS NULL
BEGIN
CREATE TABLE Attachments (
 Id int IDENTITY PRIMARY KEY, ProfileId int NOT NULL REFERENCES Profiles(Id) ON DELETE CASCADE,
 FileName nvarchar(255) NOT NULL, StoredName nvarchar(255) NOT NULL, Size bigint NOT NULL,
 ContentType nvarchar(200) NOT NULL, UploadedAt datetime2 NOT NULL
);
CREATE INDEX IX_Attachments_ProfileId ON Attachments(ProfileId);
END;

-- END CRM TABLES
