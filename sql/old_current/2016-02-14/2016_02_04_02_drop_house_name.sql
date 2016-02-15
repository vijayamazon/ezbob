SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LogicalGlueRequests') AND name = 'HouseName')
	ALTER TABLE LogicalGlueRequests DROP COLUMN HouseName
GO
