IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanOfferMultiplier]') AND type in (N'U'))
DROP TABLE [dbo].[LoanOfferMultiplier]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanOfferMultiplier](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StartScore] [int] NULL,
	[EndScore] [int] NULL,
	[Multiplier] [numeric](10, 2) NULL
) ON [PRIMARY]
GO
