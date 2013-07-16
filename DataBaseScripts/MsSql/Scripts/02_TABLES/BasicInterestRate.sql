IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BasicInterestRate]') AND type in (N'U'))
DROP TABLE [dbo].[BasicInterestRate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BasicInterestRate](
	[FromScore] [int] NOT NULL,
	[ToScore] [int] NOT NULL,
	[LoanIntrestBase] [decimal](18, 4) NOT NULL
) ON [PRIMARY]
GO
