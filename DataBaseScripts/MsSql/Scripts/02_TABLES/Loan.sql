IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Loan]') AND type in (N'U'))
DROP TABLE [dbo].[Loan]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Loan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[LoanAmount] [numeric](18, 0) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Balance] [numeric](18, 0) NOT NULL,
	[CustomerId] [int] NULL,
	[DateClosed] [datetime] NULL,
	[Repayments] [numeric](18, 0) NULL,
	[RepaymentsNum] [int] NULL,
	[OnTime] [numeric](18, 0) NULL,
	[OnTimeNum] [int] NULL,
	[Late30] [numeric](18, 0) NULL,
	[Late30Num] [int] NULL,
	[Late60] [numeric](18, 0) NULL,
	[Late60Num] [int] NULL,
	[Late90] [numeric](18, 0) NULL,
	[Late90Num] [int] NULL,
	[PastDues] [numeric](18, 0) NULL,
	[PastDuesNum] [int] NULL,
	[NextRepayment] [numeric](18, 0) NULL,
	[Position] [int] NULL,
	[Interest] [numeric](18, 0) NULL,
	[PaymentStatus] [nvarchar](50) NULL,
	[RequestCashId] [bigint] NULL,
	[RefNum] [char](11) NULL,
	[IsDefaulted] [int] NULL,
	[Late90Plus] [numeric](18, 0) NULL,
	[Late90PlusNum] [numeric](18, 0) NULL,
	[MaxDelinquencyDays] [int] NULL,
	[Principal] [decimal](18, 2) NULL,
	[InterestRate] [decimal](18, 4) NOT NULL,
	[APR] [decimal](18, 4) NULL,
	[SetupFee] [decimal](18, 4) NULL,
	[Fees] [decimal](18, 4) NULL,
	[AgreementModel] [nvarchar](max) NULL,
	[InterestPaid] [decimal](18, 4) NULL,
	[FeesPaid] [decimal](18, 4) NULL,
	[ZohoId] [nvarchar](100) NULL,
	[PayPointCardId] [int] NULL,
	[LastReportedCAISStatus] [nvarchar](50) NULL,
	[LastReportedCAISStatusDate] [datetime] NULL,
	[LoanTypeId] [int] NULL,
	[Modified] [bit] NULL,
	[LastRecalculation] [datetime] NULL,
	[InterestDue] [decimal](18, 4) NULL,
 CONSTRAINT [PK_Loan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Loan] ADD  CONSTRAINT [DF_Loan_InterestRate]  DEFAULT ((0.06)) FOR [InterestRate]
GO
