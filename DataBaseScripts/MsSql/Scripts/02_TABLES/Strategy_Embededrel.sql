IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_Embededrel]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_Embededrel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_Embededrel](
	[StrategyId] [int] NULL,
	[EmbStrategyId] [int] NULL
) ON [PRIMARY]
GO
