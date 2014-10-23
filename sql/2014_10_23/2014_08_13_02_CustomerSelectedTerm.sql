IF EXISTS (SELECT * FROM syscolumns WHERE name = 'CustomerSelectedTerm' AND id = OBJECT_ID('CashRequests'))
	ALTER TABLE CashRequests DROP COLUMN CustomerSelectedTerm
GO
