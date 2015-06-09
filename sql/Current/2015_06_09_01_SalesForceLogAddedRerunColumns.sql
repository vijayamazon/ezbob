IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('SalesForceLog') AND name='RerunDate') 
BEGIN
	ALTER TABLE SalesForceLog ADD RerunDate DATETIME
	ALTER TABLE SalesForceLog ADD RerunSuccess BIT 
END 
GO
