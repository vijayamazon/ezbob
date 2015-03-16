SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('DecisionTrailNotes') IS NULL
BEGIN
	CREATE TABLE DecisionTrailNotes (
		TrailNoteID INT NOT NULL,
		TrailID BIGINT NOT NULL,
		TrailNote NVARCHAR(4000) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionTrailNotes PRIMARY KEY (TrailNoteID),
		CONSTRAINT FK_DecisionTrailNotes FOREIGN KEY (TrailID) REFERENCES DecisionTrail (TrailID)
	)
END
GO
