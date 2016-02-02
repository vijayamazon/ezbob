SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerLogicalGlueHistory') AND name = 'SetTime')
BEGIN
	ALTER TABLE CustomerLogicalGlueHistory DROP COLUMN TimestampCounter

	ALTER TABLE CustomerLogicalGlueHistory ADD SetTime DATETIME NOT NULL

	ALTER TABLE CustomerLogicalGlueHistory ADD TimestampCounter ROWVERSION
END
GO
