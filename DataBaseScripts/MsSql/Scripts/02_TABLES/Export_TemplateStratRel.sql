IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_TemplateStratRel]') AND type in (N'U'))
DROP TABLE [dbo].[Export_TemplateStratRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Export_TemplateStratRel](
	[TemplateId] [int] NULL,
	[StrategyId] [int] NULL
) ON [PRIMARY]
GO
