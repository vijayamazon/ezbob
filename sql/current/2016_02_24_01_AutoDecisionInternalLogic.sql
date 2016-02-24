SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('I_ProductSubType') AND name = 'AutoDecisionInternalLogic')
BEGIN
	ALTER TABLE I_ProductSubType DROP COLUMN TimestampCounter

	ALTER TABLE I_ProductSubType ADD AutoDecisionInternalLogic BIT NULL

	EXECUTE('UPDATE I_ProductSubType SET AutoDecisionInternalLogic = IsRegulated')

	EXECUTE('ALTER TABLE I_ProductSubType ALTER COLUMN AutoDecisionInternalLogic BIT NOT NULL')

	ALTER TABLE I_ProductSubType ADD TimestampCounter ROWVERSION
END
GO
