IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS IC WHERE TABLE_NAME = 'MedalCoefficients' AND COLUMN_NAME = 'MedalFlow')
BEGIN
	ALTER TABLE MedalCoefficients DROP COLUMN MedalFlow
END
GO

