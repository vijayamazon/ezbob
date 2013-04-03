IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Application]') AND type in (N'U'))
DROP TABLE [dbo].[Application_Application]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_Application](
	[ApplicationId] [bigint] IDENTITY(1,1) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[CreatorUserId] [int] NOT NULL,
	[StrategyId] [int] NOT NULL,
	[LockedByUserId] [int] NULL,
	[State] [int] NOT NULL,
	[LastUpdateDate] [datetime] NULL,
	[Version] [int] NOT NULL,
	[IsTimeLimitExceeded] [bit] NOT NULL,
	[ParentAppID] [int] NULL,
	[ChildCount] [int] NULL,
	[AppCounter] [bigint] NULL,
	[ExecutionPath] [ntext] NULL,
	[ExecutionPathBin] [varbinary](max) NULL,
	[ErrorMsg] [nvarchar](3000) NULL,
	[Param1] [nvarchar](512) NULL,
	[Param2] [nvarchar](512) NULL,
	[Param3] [nvarchar](512) NULL,
	[Param4] [nvarchar](512) NULL,
	[Param5] [nvarchar](512) NULL,
	[Param6] [nvarchar](512) NULL,
	[Param7] [nvarchar](512) NULL,
	[Param8] [nvarchar](512) NULL,
	[Param9] [nvarchar](512) NULL,
 CONSTRAINT [PK_Application_Application] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Application_Application_LBU] ON [dbo].[Application_Application] 
(
	[LockedByUserId] ASC
)
INCLUDE ( [ApplicationId],
[CreationDate],
[CreatorUserId],
[StrategyId],
[Version],
[AppCounter]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Application_Application] ADD  CONSTRAINT [DF__Runtime_A__Creat__6ABAD62E]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Application_Application] ADD  CONSTRAINT [DF__Applicati__State__2D7CBDC4]  DEFAULT ((0)) FOR [State]
GO
ALTER TABLE [dbo].[Application_Application] ADD  CONSTRAINT [DF_Application_Application_Version]  DEFAULT ((0)) FOR [Version]
GO
ALTER TABLE [dbo].[Application_Application] ADD  CONSTRAINT [DF_Application_Application_IsTimeLimitExceeded]  DEFAULT ((0)) FOR [IsTimeLimitExceeded]
GO
