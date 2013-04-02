IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_PublicRel]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_PublicRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_PublicRel](
	[PUBLICID] [int] NULL,
	[STRATEGYID] [int] NULL,
	[PERCENT] [float] NULL
) ON [PRIMARY]
GO
