IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyEngine_ExecutionState]') AND type in (N'U'))
DROP TABLE [dbo].[StrategyEngine_ExecutionState]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StrategyEngine_ExecutionState](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [bigint] NOT NULL,
	[CurrentNodeId] [int] NULL,
	[CurrentNodePostfix] [nvarchar](250) NULL,
	[Data] [nvarchar](max) NULL,
	[StartTime] [datetime] NOT NULL,
	[IsTimeoutReported] [bit] NULL,
 CONSTRAINT [PK_StrategyEngine_ExecutionState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [i_se_es_1] ON [dbo].[StrategyEngine_ExecutionState] 
(
	[ApplicationId] ASC
)
INCLUDE ( [StartTime],
[CurrentNodeId],
[CurrentNodePostfix],
[Id],
[IsTimeoutReported]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
ALTER TABLE [dbo].[StrategyEngine_ExecutionState] ADD  DEFAULT (getdate()) FOR [StartTime]
GO
