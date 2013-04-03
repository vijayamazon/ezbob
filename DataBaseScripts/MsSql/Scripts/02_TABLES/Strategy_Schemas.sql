IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_Schemas]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_Schemas]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_Schemas](
	[StrategyId] [int] NOT NULL,
	[BinaryData] [varbinary](max) NULL,
 CONSTRAINT [PK_Strategy_Schemas] PRIMARY KEY CLUSTERED 
(
	[StrategyId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
