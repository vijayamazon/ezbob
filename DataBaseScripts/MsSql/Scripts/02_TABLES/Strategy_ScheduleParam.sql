IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_ScheduleParam]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_ScheduleParam]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_ScheduleParam](
	[Id] [int] IDENTITY(30,1) NOT NULL,
	[StrategyScheduleId] [int] NOT NULL,
	[CurrentVersionId] [int] NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Data] [nvarchar](max) NOT NULL,
	[Deleted] [int] NULL,
	[UserId] [int] NULL,
	[CreationDate] [datetime] NOT NULL,
	[SignedDocument] [nvarchar](max) NULL,
 CONSTRAINT [PK_ScheduleParam] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [IX_ScheduleParam] UNIQUE NONCLUSTERED 
(
	[StrategyScheduleId] ASC,
	[Name] ASC,
	[Deleted] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Strategy_ScheduleParam] ADD  CONSTRAINT [DF_ScheduleParam_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
