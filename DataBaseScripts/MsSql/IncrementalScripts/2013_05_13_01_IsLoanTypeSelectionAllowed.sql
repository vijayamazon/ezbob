IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsLoanTypeSelectionAllowed' AND id = OBJECT_ID('CashRequests'))
BEGIN
	ALTER TABLE CashRequests ADD IsLoanTypeSelectionAllowed BIT NOT NULL DEFAULT (0)
END
GO
