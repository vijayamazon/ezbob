IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_TemplateNodeRel]') AND type in (N'U'))
DROP TABLE [dbo].[Export_TemplateNodeRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Export_TemplateNodeRel](
	[TemplateId] [int] NULL,
	[NodeId] [int] NULL,
	[OutputType] [int] NULL
) ON [PRIMARY]
GO
