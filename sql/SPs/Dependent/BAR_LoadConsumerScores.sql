SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BAR_LoadConsumerScores') IS NULL
	EXECUTE('CREATE PROCEDURE BAR_LoadConsumerScores AS SELECT 1')
GO

ALTER PROCEDURE BAR_LoadConsumerScores
@TrailTagID BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ServiceLogId BIGINT
	DECLARE @ExperianConsumerDataID BIGINT
	DECLARE @ConsumerScore INT

	------------------------------------------------------------------------------

	DECLARE @CustomerID INT
	DECLARE @Now DATETIME

	------------------------------------------------------------------------------

	IF @TrailTagID IS NULL
	BEGIN
		SELECT
			@TrailTagID = MAX(TrailTagID)
		FROM
			DecisionTrailTags
		WHERE
			TrailTag LIKE '#BravoAutoRpt%'
	END

	------------------------------------------------------------------------------

	SELECT
		CustomerID = br.CustomerID,
		Now = r.UnderwriterDecisionDate,
		ConsumerScore = CONVERT(INT, 0)
	INTO
		#res
	FROM
		BAR_Results br
		INNER JOIN CashRequests r ON br.FirstCashRequestID = r.Id
	WHERE
		br.TrailTagID = @TrailTagID
		AND
		br.HasEnoughData = 1
		AND
		br.HasSignature = 0

	------------------------------------------------------------------------------

	DECLARE cur CURSOR FOR SELECT CustomerID, Now FROM #res
	OPEN cur

	------------------------------------------------------------------------------

	FETCH NEXT FROM cur INTO @CustomerID, @Now

	------------------------------------------------------------------------------

	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT, @Now

		-------------------------------------------------------------------------

		SELECT
			@ExperianConsumerDataID = e.Id
		FROM
			ExperianConsumerData e
		WHERE
			e.ServiceLogId = @ServiceLogId

		-------------------------------------------------------------------------

		SET @ConsumerScore = ISNULL((
			SELECT
				MIN(x.ExperianConsumerScore)
			FROM	(
				SELECT ISNULL(d.BureauScore, 0) AS ExperianConsumerScore
				FROM ExperianConsumerData d
				INNER JOIN MP_ServiceLog l ON d.ServiceLogId = l.Id
				WHERE d.Id = @ExperianConsumerDataID
				AND l.InsertDate < @Now

				UNION

				SELECT ISNULL(d.MinScore, 0) AS ExperianConsumerScore
				FROM CustomerAnalyticsDirector d
				WHERE d.CustomerID = @CustomerID
				AND d.AnalyticsDate < @Now
			) x
		), 0)

		-------------------------------------------------------------------------

		UPDATE #res SET ConsumerScore = @ConsumerScore WHERE CustomerID = @CustomerID

		-------------------------------------------------------------------------

		FETCH NEXT FROM cur INTO @CustomerID, @Now
	END

	------------------------------------------------------------------------------

	CLOSE cur
	DEALLOCATE cur

	------------------------------------------------------------------------------

	SELECT
		CustomerID,
		ConsumerScore
	FROM
		#res

	------------------------------------------------------------------------------

	DROP TABLE #res
END
GO
