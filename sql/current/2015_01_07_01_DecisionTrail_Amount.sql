SET QUOTED_IDENTIFIER ON
GO

DECLARE @Changed BIT = 0

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'Amount')
BEGIN
	SET @Changed = 1

	ALTER TABLE DecisionTrail ADD Amount NUMERIC(18, 2) NULL
END

IF @Changed = 1
BEGIN
	ALTER TABLE DecisionTrail DROP COLUMN TimestampCounter

	ALTER TABLE DecisionTrail ADD TimestampCounter ROWVERSION
END
GO
