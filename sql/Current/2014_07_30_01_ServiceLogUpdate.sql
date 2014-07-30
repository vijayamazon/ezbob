IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Firstname' AND id = OBJECT_ID('MP_ServiceLog'))
BEGIN
	ALTER TABLE MP_ServiceLog ADD Firstname NVARCHAR(50) NULL
	ALTER TABLE MP_ServiceLog ADD Surname NVARCHAR(50) NULL
	ALTER TABLE MP_ServiceLog ADD DateOfBirth DATETIME NULL
	ALTER TABLE MP_ServiceLog ADD Postcode NVARCHAR(50) NULL
END	
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'ExperianConsumerScore' AND id = OBJECT_ID('Customer'))
BEGIN
	ALTER TABLE Customer ADD ExperianConsumerScore INT NULL
	ALTER TABLE Director ADD ExperianConsumerScore INT NULL
END

GO
	
	