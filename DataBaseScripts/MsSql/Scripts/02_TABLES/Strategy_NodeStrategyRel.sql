IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRel]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_NodeStrategyRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_NodeStrategyRel](
	[StrategyId] [int] NOT NULL,
	[NodeId] [int] NOT NULL
) ON [PRIMARY]
GO
