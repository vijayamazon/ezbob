SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BAR_LoadPersonalDelayData') IS NULL
	EXECUTE('CREATE PROCEDURE BAR_LoadPersonalDelayData AS SELECT 1')
GO

ALTER PROCEDURE BAR_LoadPersonalDelayData
@TrailTagID BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;

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
		Now = r.UnderwriterDecisionDate
	INTO
		#src
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

	SELECT
		CustomerID = CONVERT(INT, NULL),
		cais.WorstStatus,
		cais.Balance,
		cais.CurrentDefBalance,
		cais.LastUpdatedDate,
		DecisionTime = CONVERT(DATETIME, NULL)
	INTO
		#res
	FROM
		ExperianConsumerDataCais cais
	WHERE
		1 = 0

	------------------------------------------------------------------------------

	DECLARE cur CURSOR FOR SELECT CustomerID, Now FROM #src
	OPEN cur

	------------------------------------------------------------------------------

	FETCH NEXT FROM cur INTO @CustomerID, @Now

	------------------------------------------------------------------------------

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #res (CustomerID, WorstStatus, Balance, CurrentDefBalance, LastUpdatedDate, DecisionTime)
		SELECT
			@CustomerID,
			r.WorstStatus,
			r.Balance,
			r.CurrentDefBalance,
			r.LastUpdatedDate,
			@Now
		FROM
			ExperianConsumerDataCais r
		WHERE
			r.ExperianConsumerDataId = dbo.udfLoadExperianConsumerIdForCustomerAndDate(@CustomerId, @Now)

		-------------------------------------------------------------------------

		FETCH NEXT FROM cur INTO @CustomerID, @Now
	END

	------------------------------------------------------------------------------

	CLOSE cur
	DEALLOCATE cur

	------------------------------------------------------------------------------

	SELECT
		r.CustomerID,
		r.WorstStatus,
		r.Balance,
		r.CurrentDefBalance,
		r.LastUpdatedDate,
		r.DecisionTime,
		LoanCount = (SELECT COUNT(*) FROM Loan l WHERE l.CustomerId = r.CustomerID AND l.[Date] < r.DecisionTime)
	FROM
		#res r
	ORDER BY
		r.CustomerID,
		r.LastUpdatedDate

	------------------------------------------------------------------------------

	DROP TABLE #res
	DROP TABLE #src
END
GO
