IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=OBJECT_ID('Broker') AND name='LicenseNumber')
BEGIN
	ALTER TABLE Broker ADD LicenseNumber NVARCHAR(50)
END
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=OBJECT_ID('Broker') AND name='FCARegistered')
BEGIN
	ALTER TABLE Broker ADD FCARegistered BIT
END
GO
