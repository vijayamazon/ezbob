SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('Decisions') IS NULL
BEGIN
	CREATE TABLE Decisions (
		DecisionID INT NOT NULL,
		DecisionName NVARCHAR(64) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_Decision PRIMARY KEY (DecisionID),
		CONSTRAINT CHK_Decision CHECK (LTRIM(RTRIM(DecisionName)) != '')
	)
END
GO

-------------------------------------------------------------------------------

SELECT -- instead of CREATE TABLE to preserve collation
	DecisionID,
	DecisionName
INTO
	#d
FROM
	Decisions
WHERE
	1 = 0
GO

-------------------------------------------------------------------------------

INSERT INTO #d (DecisionID, DecisionName) VALUES
	(1, 'Approve'),
	(2, 'Reject'),
	(3, 'Escalate'),
	(4, 'Pending'),
	(5, 'Waiting'),
	(6, 'ReApprove'),
	(7, 'ReReject')
GO

-------------------------------------------------------------------------------

INSERT INTO Decisions (DecisionID, DecisionName)
SELECT
	n.DecisionID,
	n.DecisionName
FROM
	#d n
	LEFT JOIN Decisions e ON n.DecisionID = e.DecisionID
WHERE
	e.DecisionID IS NULL
GO

-------------------------------------------------------------------------------

DROP TABLE #d
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('DecisionTrail') IS NULL
BEGIN
	CREATE TABLE DecisionTrail (
		TrailID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		DecisionID INT NOT NULL,
		DecisionTime DATETIME NOT NULL,
		UniqueID UNIQUEIDENTIFIER NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DecisionTrail PRIMARY KEY (TrailID),
		CONSTRAINT FK_DecisionTrail_Decision FOREIGN KEY (DecisionID) REFERENCES Decisions (DecisionID)
	)
END
GO
