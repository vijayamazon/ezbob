SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LogicalGlueResponses') AND name = 'Reason')
BEGIN
	ALTER TABLE LogicalGlueResponses DROP COLUMN TimestampCounter

	ALTER TABLE LogicalGlueResponses ADD Reason NVARCHAR(MAX) NULL
	ALTER TABLE LogicalGlueResponses ADD Outcome NVARCHAR(MAX) NULL

	ALTER TABLE LogicalGlueResponses ADD TimestampCounter ROWVERSION
END
GO
