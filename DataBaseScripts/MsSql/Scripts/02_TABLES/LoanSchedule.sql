IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanSchedule]') AND type in (N'U'))
DROP TABLE [dbo].[LoanSchedule]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanSchedule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[RepaymentAmount] [numeric](18, 2) NOT NULL,
	[Interest] [numeric](18, 2) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[LateCharges] [numeric](18, 2) NOT NULL,
	[AmountDue] [numeric](18, 2) NOT NULL,
	[LoanId] [int] NOT NULL,
	[Position] [int] NULL,
	[Principal] [numeric](18, 2) NULL,
	[Balance] [decimal](18, 2) NULL,
	[LoanRepayment] [decimal](18, 2) NULL,
	[Delinquency] [int] NULL,
	[Fees] [decimal](18, 4) NULL,
	[TwoDaysDueMailSent] [bit] NULL,
	[TwoWeeksDueMailSent] [bit] NULL,
	[InterestPaid] [decimal](18, 4) NULL,
	[FeesPaid] [decimal](18, 4) NULL,
	[InterestRate] [decimal](18, 4) NULL,
 CONSTRAINT [PK_LoanSchedule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_LoanSchedule_Date] ON [dbo].[LoanSchedule] 
(
	[Status] ASC
)
INCLUDE ( [Date]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_LoanSchedule_LoanId] ON [dbo].[LoanSchedule] 
(
	[Status] ASC
)
INCLUDE ( [Date],
[LoanId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
