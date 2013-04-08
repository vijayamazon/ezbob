IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_Schedule]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_Schedule]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_Schedule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[StrategyId] [int] NOT NULL,
	[ScheduleType] [int] NOT NULL,
	[ExecutionType] [int] NOT NULL,
	[ScheduleMask] [nvarchar](512) NOT NULL,
	[NextRun] [datetime] NOT NULL,
	[CreatorUserId] [int] NULL,
	[IsPaused] [bit] NULL,
 CONSTRAINT [PK_Strategy_Schedule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Strategy_Schedule] ADD  DEFAULT ((0)) FOR [ScheduleType]
GO
ALTER TABLE [dbo].[Strategy_Schedule] ADD  DEFAULT ((0)) FOR [ExecutionType]
GO
