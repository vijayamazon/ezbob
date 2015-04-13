SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------
--
-- MatrixColumns table
--
-------------------------------------------------------------------------------

IF OBJECT_ID('CHK_MatrixColumns') IS NOT NULL
	ALTER TABLE MatrixColumns DROP CONSTRAINT CHK_MatrixColumns
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UC_MatrixColumns') IS NOT NULL
	ALTER TABLE MatrixColumns DROP CONSTRAINT UC_MatrixColumns
GO

-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MatrixColumns') AND name = 'PositiveInfinity')
	ALTER TABLE MatrixColumns DROP COLUMN PositiveInfinity
GO

-------------------------------------------------------------------------------

ALTER TABLE MatrixColumns ADD CONSTRAINT UC_MatrixColumns UNIQUE (MatrixRowID, TitleValue)
GO

-------------------------------------------------------------------------------
--
-- MatrixRowTitles table
--
-------------------------------------------------------------------------------

IF OBJECT_ID('CHK_MatrixRowTitles') IS NOT NULL
	ALTER TABLE MatrixRowTitles DROP CONSTRAINT CHK_MatrixRowTitles
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UC_MatrixRowTitles') IS NOT NULL
	ALTER TABLE MatrixRowTitles DROP CONSTRAINT UC_MatrixRowTitles
GO

-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MatrixRowTitles') AND name = 'PositiveInfinity')
	ALTER TABLE MatrixRowTitles DROP COLUMN PositiveInfinity
GO

-------------------------------------------------------------------------------

ALTER TABLE MatrixRowTitles ADD CONSTRAINT UC_MatrixRowTitles UNIQUE (MatrixID, TitleValue)
GO

-------------------------------------------------------------------------------
--
-- Matrices table
--
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Matrices') AND name = 'MinRowTitleValue')
BEGIN
	ALTER TABLE Matrices DROP COLUMN TimestampCounter

	ALTER TABLE Matrices ADD MinRowTitleValue DECIMAL(18, 6) NULL

	ALTER TABLE Matrices ADD TimestampCounter ROWVERSION
END
GO

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Matrices') AND name = 'MinColumnTitleValue')
BEGIN
	ALTER TABLE Matrices DROP COLUMN TimestampCounter

	ALTER TABLE Matrices ADD MinColumnTitleValue DECIMAL(18, 6) NULL

	ALTER TABLE Matrices ADD TimestampCounter ROWVERSION
END
GO
