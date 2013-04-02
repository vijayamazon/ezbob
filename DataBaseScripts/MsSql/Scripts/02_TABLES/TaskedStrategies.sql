IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaskedStrategies]') AND type in (N'U'))
DROP TABLE [dbo].[TaskedStrategies]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaskedStrategies](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Label] [nvarchar](64) NOT NULL,
	[StrategyId] [int] NOT NULL,
	[TaskId] [int] NOT NULL,
 CONSTRAINT [PK_TASKEDSTRATEGY] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
