SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfGetCustomerScoreAnalytics') IS NOT NULL
	DROP FUNCTION dbo.udfGetCustomerScoreAnalytics
GO

CREATE FUNCTION dbo.udfGetCustomerScoreAnalytics(@CustomerID INT, @Now DATETIME)
RETURNS @output TABLE (
	CustomerID INT NULL,
	Score INT NULL,
	MinScore INT NULL,
	MaxScore INT NULL
)
AS
BEGIN
	DECLARE @data CustomerAndDirectorsServiceLogIDs

	INSERT INTO @output (CustomerID, Score, MinScore, MaxScore)
		VALUES (@CustomerID, 0, 0, 0)

	INSERT INTO @data (
		CustomerID,
		DirectorID,
		ServiceLogID,
		ExperianConsumerDataID
	) SELECT
		CustomerID,
		DirectorID,
		ServiceLogID,
		ExperianConsumerDataID
	FROM
		dbo.udfLoadCustomerAndDirectorsServiceLog(@CustomerID, @Now)

	DECLARE @scores AS TABLE (
		CustomerID INT NULL,
		DirectorID INT NULL,
		Score INT
	)

	INSERT INTO @scores (CustomerID, DirectorID, Score)
	SELECT
		d.CustomerID,
		d.DirectorID,
		Score = ISNULL(e.BureauScore, 0)
	FROM
		@data d
		INNER JOIN ExperianConsumerData e ON d.ExperianConsumerDataID = e.Id

	UPDATE @output SET Score = ISNULL((SELECT Score FROM @scores WHERE DirectorID IS NULL), 0)

	UPDATE @output SET MinScore = ISNULL((SELECT MIN(Score) FROM @scores), 0)

	UPDATE @output SET MaxScore = ISNULL((SELECT MAX(Score) FROM @scores), 0)

	RETURN
END
GO
