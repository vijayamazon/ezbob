IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalAggregationFormula]') AND type in (N'U'))
DROP TABLE [dbo].[MP_PayPalAggregationFormula]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPalAggregationFormula](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FormulaNum] [int] NOT NULL,
	[FormulaName] [nvarchar](300) NOT NULL,
	[Type] [nvarchar](300) NOT NULL,
	[Status] [nvarchar](300) NOT NULL,
	[Positive] [bit] NULL
) ON [PRIMARY]
GO
