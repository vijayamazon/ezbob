ALTER TABLE dbo.Loan ADD
	InterestPaid decimal(18, 4) NULL
GO
ALTER TABLE dbo.LoanSchedule ADD
	InterestPaid decimal(18, 4) NULL
GO

ALTER TABLE dbo.Loan ADD
	FeesPaid decimal(18, 4) NULL
GO
ALTER TABLE dbo.LoanSchedule ADD
	FeesPaid decimal(18, 4) NULL
GO