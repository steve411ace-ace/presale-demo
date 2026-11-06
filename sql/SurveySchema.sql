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

IF OBJECT_ID(N'dbo.SurveyPresentations', N'U') IS NULL
CREATE TABLE [dbo].[SurveyPresentations] (
[Id] int IDENTITY(1,1) PRIMARY KEY,
[MediaUrl] nvarchar(max) NULL,
[MediaType] nvarchar(max) NULL
);
