IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MP_ServiceLog') AND name = 'DirectorId')
	ALTER TABLE MP_ServiceLog ADD DirectorId int
GO