SET QUOTED_IDENTIFIER ON
GO

DECLARE @t INT

SELECT
	@t = xtype
FROM
	syscolumns
WHERE
	id = OBJECT_ID('DecisionTrailNotes')
	AND
	name = 'TrailNoteID'

IF @t != TYPE_ID('BIGINT')
BEGIN
	ALTER TABLE DecisionTrailNotes DROP CONSTRAINT PK_DecisionTrailNotes

	ALTER TABLE DecisionTrailNotes DROP COLUMN TrailNoteID

	ALTER TABLE DecisionTrailNotes ADD TrailNoteID BIGINT IDENTITY(1, 1) NOT NULL

	ALTER TABLE DecisionTrailNotes ADD CONSTRAINT PK_DecisionTrailNotes PRIMARY KEY (TrailNoteID)

	ALTER TABLE DecisionTrailNotes DROP COLUMN TimestampCounter

	ALTER TABLE DecisionTrailNotes ADD TimestampCounter ROWVERSION
END
GO
