IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'DirectorId' AND id = OBJECT_ID('MP_ExperianHistory'))
BEGIN
	ALTER TABLE MP_ExperianHistory ADD DirectorId INT NULL
	ALTER TABLE MP_ExperianHistory ADD CompanyRefNum NVARCHAR(20) NULL
	DELETE FROM MP_ExperianHistory
END	
GO