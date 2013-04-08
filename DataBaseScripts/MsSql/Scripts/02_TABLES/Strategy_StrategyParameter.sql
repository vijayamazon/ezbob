IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyParameter]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_StrategyParameter]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_StrategyParameter](
	[StratParamId] [int] IDENTITY(1,1) NOT NULL,
	[TypeId] [int] NOT NULL,
	[OwnerId] [int] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsInput] [bit] NOT NULL,
	[IsOutput] [bit] NOT NULL,
 CONSTRAINT [PK_Strategy_StrategyParameter] PRIMARY KEY CLUSTERED 
(
	[StratParamId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [IX_Strategy_StrategyParameter] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[OwnerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
