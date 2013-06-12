ALTER TABLE dbo.Loan ADD
	LastRecalculation datetime NULL,
	InterestDue decimal(18, 4) NULL
GO

ALTER TABLE dbo.Loan SET (LOCK_ESCALATION = TABLE)
GO