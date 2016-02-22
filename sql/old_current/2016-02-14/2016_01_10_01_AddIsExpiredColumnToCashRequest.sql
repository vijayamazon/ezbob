IF NOT EXISTS (SELECT * FROM syscolumns WHERE name ='IsExpired' AND id=object_id('CashRequests'))
BEGIN
	ALTER TABLE CashRequests ADD IsExpired BIT NOT NULL DEFAULT(0)
END
GO

