IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[needed_appId]') AND type in (N'U'))
DROP TABLE [dbo].[needed_appId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[needed_appId](
	[StrategyId] [int] NOT NULL
) ON [PRIMARY]
GO
