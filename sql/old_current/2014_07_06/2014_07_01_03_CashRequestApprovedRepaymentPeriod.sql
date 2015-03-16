IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'ApprovedRepaymentPeriod' AND id = OBJECT_ID('CashRequests'))
	ALTER TABLE CashRequests ADD ApprovedRepaymentPeriod INT
GO

UPDATE CashRequests SET ApprovedRepaymentPeriod = CashRequests.RepaymentPeriod WHERE ApprovedRepaymentPeriod IS NULL

GO
