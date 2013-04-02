IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_CalendarRelation]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_CalendarRelation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_CalendarRelation](
	[StrategyId] [int] NOT NULL,
	[CalendarId] [int] NOT NULL
) ON [PRIMARY]
GO
