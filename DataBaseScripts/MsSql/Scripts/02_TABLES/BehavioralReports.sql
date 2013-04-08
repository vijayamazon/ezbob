IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BehavioralReports]') AND type in (N'U'))
DROP TABLE [dbo].[BehavioralReports]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BehavioralReports](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[StrategyId] [int] NULL,
	[Name] [nvarchar](1024) NULL,
	[TypeId] [int] NULL,
	[Path] [nvarchar](2048) NULL,
	[CreationDate] [datetime] NULL,
	[TestRun] [int] NULL,
	[IsNotRead] [int] NULL,
 CONSTRAINT [PK_BehavioralReports] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
