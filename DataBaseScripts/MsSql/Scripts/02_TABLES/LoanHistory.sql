IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanHistory]') AND type in (N'U'))
DROP TABLE [dbo].[LoanHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanHistory](
	[Id] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Status] [nchar](50) NOT NULL,
	[Balance] [decimal](18, 4) NOT NULL,
	[Interest] [decimal](18, 4) NOT NULL,
	[Principal] [decimal](18, 4) NOT NULL,
	[Fees] [decimal](18, 4) NOT NULL,
	[LoanId] [int] NULL,
	[ExpectedPrincipal] [decimal](18, 4) NULL,
	[ExpectedInterest] [decimal](18, 4) NULL,
	[ExpectedFees] [decimal](18, 4) NULL,
	[ExpectedAmountDue] [decimal](18, 4) NULL,
 CONSTRAINT [PK_LoanHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_LoanHistory_Date] ON [dbo].[LoanHistory] 
(
	[Date] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
