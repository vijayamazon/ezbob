SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('DecisionTrailStepTimeNames') IS NULL
BEGIN
	CREATE TABLE DecisionTrailStepTimeNames (
		StepTimeNameID BIGINT NOT NULL,
		StepTimeName NVARCHAR(64) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionTrailStepTimeNames PRIMARY KEY (StepTimeNameID)
	)
END
GO

-------------------------------------------------------------------------------

SELECT
	StepTimeNameID,
	StepTimeName
INTO
	#t
FROM
	DecisionTrailStepTimeNames
WHERE
	1 = 0
GO

-------------------------------------------------------------------------------

INSERT INTO #t (StepTimeName, StepTimeNameID) VALUES
	('Creation',      1),
	('Initializtion', 2),
	('GatherData',    3),
	('RunCheck',      4),
	('MakeDecision',  5)

-------------------------------------------------------------------------------

INSERT INTO DecisionTrailStepTimeNames (StepTimeNameID, StepTimeName)
SELECT
	t.StepTimeNameID,
	t.StepTimeName
FROM
	#t t
	LEFT JOIN DecisionTrailStepTimeNames n ON t.StepTimeNameID = n.StepTimeNameID
WHERE
	n.StepTimeNameID IS NULL

-------------------------------------------------------------------------------

DROP TABLE #t
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('DecisionTrailStepTimes') IS NULL
BEGIN
	CREATE TABLE DecisionTrailStepTimes (
		StepTimeID BIGINT IDENTITY(1, 1) NOT NULL,
		TrailID BIGINT NOT NULL,
		Position INT NOT NULL,
		StepTimeNameID BIGINT NOT NULL,
		StepLength FLOAT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionTrailStepTimes PRIMARY KEY (StepTimeID),
		CONSTRAINT FK_DecisionTrailStepTimes_Name FOREIGN KEY (StepTimeNameID) REFERENCES DecisionTrailStepTimeNames (StepTimeNameID)
	)
END
GO
