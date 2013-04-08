IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyAccountRel]') AND type in (N'U'))
DROP TABLE [dbo].[StrategyAccountRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StrategyAccountRel](
	[AccountId] [int] NOT NULL,
	[StrategyId] [int] NOT NULL,
 CONSTRAINT [PK_StrategyAccountRel] PRIMARY KEY CLUSTERED 
(
	[AccountId] ASC,
	[StrategyId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
