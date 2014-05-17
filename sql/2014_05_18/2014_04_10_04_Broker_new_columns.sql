IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Broker') AND name = 'IsTest')
BEGIN
	ALTER TABLE Broker ADD IsTest BIT NOT NULL CONSTRAINT DF_Broker_IsTest DEFAULT(0)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Broker') AND name = 'ReferredBy')
BEGIN
	ALTER TABLE Broker ADD ReferredBy NVARCHAR(255) NULL
END
GO
