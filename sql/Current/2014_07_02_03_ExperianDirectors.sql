IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'DirectorRefNum' AND id = OBJECT_ID('ExperianDirectors'))
	ALTER TABLE ExperianDirectors ADD DirectorRefNum NVARCHAR(512) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'TimestampCounter' AND id = OBJECT_ID('ExperianDirectors'))
	ALTER TABLE ExperianDirectors ADD TimestampCounter ROWVERSION
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Line1' AND id = OBJECT_ID('ExperianDirectors'))
	ALTER TABLE ExperianDirectors ADD Line1 NVARCHAR(512) NOT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Line2' AND id = OBJECT_ID('ExperianDirectors'))
	ALTER TABLE ExperianDirectors ADD Line2 NVARCHAR(512) NOT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Line3' AND id = OBJECT_ID('ExperianDirectors'))
	ALTER TABLE ExperianDirectors ADD Line3 NVARCHAR(512) NOT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Town' AND id = OBJECT_ID('ExperianDirectors'))
	ALTER TABLE ExperianDirectors ADD Town NVARCHAR(512) NOT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'County' AND id = OBJECT_ID('ExperianDirectors'))
	ALTER TABLE ExperianDirectors ADD County NVARCHAR(512) NOT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Postcode' AND id = OBJECT_ID('ExperianDirectors'))
	ALTER TABLE ExperianDirectors ADD Postcode NVARCHAR(512) NOT NULL
GO

IF EXISTS (SELECT * FROM syscolumns WHERE name = 'AddressID' AND id = OBJECT_ID('ExperianDirectors'))
BEGIN
	ALTER TABLE ExperianDirectors DROP CONSTRAINT FK_ExperianDirector_Address
	ALTER TABLE ExperianDirectors DROP COLUMN AddressID
END
GO
