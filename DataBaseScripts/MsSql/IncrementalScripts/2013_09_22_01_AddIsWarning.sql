IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsWarning' and Object_ID = Object_ID(N'CustomerStatuses'))    
BEGIN
	ALTER TABLE CustomerStatuses ADD IsWarning BIT DEFAULT 0 NOT NULL
END
GO

UPDATE CustomerStatuses SET IsWarning = 1 WHERE Name = 'Default' OR Name = 'Risky' OR Name = 'Bad'
GO
