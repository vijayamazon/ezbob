IF OBJECT_ID('GetExperianMinMaxConsumerDirectorsScore') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianMinMaxConsumerDirectorsScore AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetExperianMinMaxConsumerDirectorsScore
@CustomerId INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ServiceLogId BIGINT

	EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT, @Now

	------------------------------------------------------------------------------

	DECLARE @ExperianConsumerDataID BIGINT

	SELECT
		@ExperianConsumerDataID = e.Id
	FROM
		ExperianConsumerData e
	WHERE
		e.ServiceLogId = @ServiceLogId

	------------------------------------------------------------------------------

	SELECT
		MIN(x.ExperianConsumerScore) AS MinExperianScore,
		MAX(x.ExperianConsumerScore) AS MaxExperianScore
	FROM	(
		SELECT ISNULL(d.BureauScore, 0) AS ExperianConsumerScore
		FROM ExperianConsumerData d
		WHERE d.Id = @ExperianConsumerDataID

		UNION

		SELECT ISNULL(d.MinScore, 0) AS ExperianConsumerScore
		FROM CustomerAnalyticsDirector d
		WHERE d.CustomerID = @CustomerID
		AND d.AnalyticsDate < @Now
	) x
END
GO
