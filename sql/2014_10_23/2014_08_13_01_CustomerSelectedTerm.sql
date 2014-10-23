IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CustomerSelectedTerm' AND id = OBJECT_ID('CashRequests'))
	ALTER TABLE CashRequests ADD CustomerSelectedTerm INT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CustomerSelectedTerm' AND id = OBJECT_ID('Loan'))
	ALTER TABLE Loan ADD CustomerSelectedTerm INT NULL
GO
