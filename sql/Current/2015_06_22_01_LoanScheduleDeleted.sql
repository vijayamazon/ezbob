
IF OBJECT_ID('LoanScheduleDeleted') IS NULL
BEGIN
CREATE TABLE [dbo].[LoanScheduleDeleted](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoanScheduleID] [int] NOT NULL,
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
	[InterestRate] [numeric](18, 7) NULL,
	[FiveDaysDueMailSent] [bit] NULL,
	[CustomInstallmentDate] [date] NULL,
	[LastNoticeSent] [bit] NULL,
	[TimestampCounter] [timestamp] NOT NULL,
	[DatePaid] [datetime] NULL,
 CONSTRAINT [PK_LoanScheduleDeleted] PRIMARY KEY CLUSTERED ([Id] ASC)
) ;
END
GO

