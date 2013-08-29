IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsEnabled' and Object_ID = Object_ID(N'CustomerStatuses'))    
BEGIN
	ALTER TABLE CustomerStatuses ADD IsEnabled BIT NOT NULL DEFAULT 0
END
GO

UPDATE CustomerStatuses SET IsEnabled=1 WHERE Name = 'Enabled' OR Name = 'Fraud Suspect' OR Name = 'Risky' OR Name = 'Bad'
GO
