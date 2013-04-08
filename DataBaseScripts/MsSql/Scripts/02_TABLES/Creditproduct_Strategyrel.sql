IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Creditproduct_Strategyrel]') AND type in (N'U'))
DROP TABLE [dbo].[Creditproduct_Strategyrel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Creditproduct_Strategyrel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreditProductId] [int] NULL,
	[StrategyId] [int] NULL,
 CONSTRAINT [PK_Creditproduct_Strategyrel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
