ALTER TABLE dbo.LoanHistory ADD
	ExpectedPrincipal decimal(18, 4) NULL,
	ExpectedInterest decimal(18, 4) NULL,
	ExpectedFees decimal(18, 4) NULL,
	ExpectedAmountDue decimal(18, 4) NULL
GO