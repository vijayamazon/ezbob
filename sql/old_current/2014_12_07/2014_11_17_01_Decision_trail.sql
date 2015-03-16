SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UC_Decision') IS NULL
	ALTER TABLE Decisions ADD CONSTRAINT UC_Decision UNIQUE (DecisionName)
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('DecisionStatuses') IS NULL
BEGIN
	CREATE TABLE DecisionStatuses (
		DecisionStatusID INT NOT NULL,
		DecisionStatus NVARCHAR(32) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionStatuses PRIMARY KEY (DecisionStatusID),
		CONSTRAINT CHK_DecisionStatuses CHECK (LTRIM(RTRIM(DecisionStatus)) != ''),
		CONSTRAINT UC_DecisionStatuses UNIQUE (DecisionStatus)
	)
END
GO

-------------------------------------------------------------------------------

SELECT -- instead of CREATE TABLE to preserve collation
	DecisionStatusID,
	DecisionStatus
INTO
	#d
FROM
	DecisionStatuses
WHERE
	1 = 0
GO

-------------------------------------------------------------------------------

INSERT INTO #d (DecisionStatusID, DecisionStatus) VALUES
	(0, 'Dunno'),
	(1, 'Affirmative'),
	(2, 'Negative')
GO

-------------------------------------------------------------------------------

INSERT INTO DecisionStatuses (DecisionStatusID, DecisionStatus)
SELECT
	n.DecisionStatusID,
	n.DecisionStatus
FROM
	#d n
	LEFT JOIN DecisionStatuses e ON n.DecisionStatusID = e.DecisionStatusID
WHERE
	e.DecisionStatusID IS NULL
GO

-------------------------------------------------------------------------------

DROP TABLE #d
GO

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'DecisionStatusID')
BEGIN
	ALTER TABLE DecisionTrail ADD DecisionStatusID INT NOT NULL
	
	ALTER TABLE DecisionTrail ADD CONSTRAINT FK_DecisionTrail_Status FOREIGN KEY (DecisionStatusID) REFERENCES DecisionStatuses (DecisionStatusID)
END
GO

-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'CompletedSuccessfully')
	ALTER TABLE DecisionTrace DROP COLUMN CompletedSuccessfully
GO

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'DecisionStatusID')
BEGIN
	ALTER TABLE DecisionTrace ADD DecisionStatusID INT NOT NULL
	
	ALTER TABLE DecisionTrace ADD CONSTRAINT FK_DecisionTrace_Status FOREIGN KEY (DecisionStatusID) REFERENCES DecisionStatuses (DecisionStatusID)
END
GO
