IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'Originator')
BEGIN
	ALTER TABLE CashRequests ADD Originator NVARCHAR(30)
END
GO
