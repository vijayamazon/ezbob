IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CompanyRefNum' AND id = OBJECT_ID('MP_ServiceLog'))
	ALTER TABLE MP_ServiceLog ADD CompanyRefNum NVARCHAR(50) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CompanyID' AND id = OBJECT_ID('MP_ServiceLog'))
	ALTER TABLE MP_ServiceLog ADD CompanyID INT NULL
GO
