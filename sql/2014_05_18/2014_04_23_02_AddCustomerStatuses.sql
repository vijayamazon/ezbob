IF NOT EXISTS (SELECT 1 FROM CustomerStatuses WHERE Name='WriteOff')
BEGIN
	INSERT INTO CustomerStatuses (Id, Name, IsEnabled, IsWarning)
		VALUES(8, 'WriteOff', 0, 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM CustomerStatuses WHERE Name='Debt Management')
BEGIN
	INSERT INTO CustomerStatuses (Id, Name, IsEnabled, IsWarning)
		VALUES(9, 'Debt Management', 0, 1)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerStatuses') AND name = 'IsDefault')
BEGIN
	ALTER TABLE CustomerStatuses ADD IsDefault BIT
END
GO

UPDATE CustomerStatuses SET IsDefault = 0
GO

UPDATE CustomerStatuses SET IsDefault = 1 WHERE Name IN ('WriteOff', 'Debt Management', 'Legal', 'Default')
GO
