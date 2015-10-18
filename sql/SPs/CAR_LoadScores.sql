SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CAR_LoadScores') IS NULL
	EXECUTE('CREATE PROCEDURE CAR_LoadScores AS SELECT 1')
GO

ALTER PROCEDURE CAR_LoadScores
@TrailTagID BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF @TrailTagID IS NULL
	BEGIN
		SELECT
			@TrailTagID = MAX(TrailTagID)
		FROM
			DecisionTrailTags
		WHERE
			TrailTag LIKE '#BravoAutoRpt%'
	END

	SELECT
		br.CustomerID,
		DecisionTime = r.UnderwriterDecisionDate,
		ConsumerScore = CONVERT(INT, NULL),
		BusinessScore = CONVERT(INT, NULL)
	INTO
		#lst
	FROM
		BAR_Results br
		INNER JOIN CashRequests r ON br.FirstCashRequestID = r.Id
	WHERE
		br.TrailTagID = @TrailTagID
		AND
		br.ManualDecisionID = 1
		AND
		br.AutoDecisionID NOT IN (1, 6)

	DECLARE @CustomerID INT
	DECLARE @Now DATETIME

	DECLARE cur CURSOR FOR SELECT CustomerID, DecisionTime FROM #lst
	OPEN cur

	FETCH NEXT FROM cur INTO @CustomerID, @Now

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-----------------------------------------------------------------------------

		DECLARE @ServiceLogId BIGINT

		-----------------------------------------------------------------------------

		EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT, @Now

		-----------------------------------------------------------------------------

		DECLARE @ExperianConsumerDataID BIGINT

		-----------------------------------------------------------------------------

		SELECT
			@ExperianConsumerDataID = e.Id
		FROM
			ExperianConsumerData e
		WHERE
			e.ServiceLogId = @ServiceLogId

		-----------------------------------------------------------------------------

		DECLARE @ConsumerScore INT = (
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
		)

		-----------------------------------------------------------------------------

		DECLARE @CompanyScore INT = 0
		DECLARE @IncorporationDate DATETIME = NULL

		-----------------------------------------------------------------------------

		EXECUTE GetCompanyHistoricalScoreAndIncorporationDate
			@CustomerId,
			1,
			@Now,
			@CompanyScore OUTPUT,
			@IncorporationDate OUTPUT

		-----------------------------------------------------------------------------

		UPDATE #lst SET
			ConsumerScore = @ConsumerScore,
			BusinessScore = @CompanyScore
		WHERE
			CustomerID = @CustomerID
			AND
			DecisionTime = @Now

		-----------------------------------------------------------------------------

		FETCH NEXT FROM cur INTO @CustomerID, @Now
	END

	CLOSE cur
	DEALLOCATE cur

	SELECT
		CustomerID,
		DecisionTime,
		ConsumerScore,
		BusinessScore
	FROM
		#lst

	DROP TABLE #lst
END
GO
