IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptMarketPlacesStats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptMarketPlacesStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptMarketPlacesStats
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	CREATE TABLE #tmp (
		MarketPlaceId INT,
		NumOfShopsDidntFinish INT,
		AvgTurnoverDidntFinish NUMERIC(18,2),
		NumOfShopsFinish INT,
		AvgTurnoverFinish NUMERIC(18,2),
		AvgScore NUMERIC(15,2),
		PercentMen NUMERIC(5,2),
		AvgAge NUMERIC(5,2),
		PercentApproved NUMERIC(5,2),
		AvgAmountApproved NUMERIC(10,2),
		NumOfShopsApproved INT
	)

	CREATE TABLE #tmp2 (
		CustomerId INT,
		MarketPlaceTypeId INT,
		NumOfShops INT,
		TurnoverForType NUMERIC(18,2),
		TotalTurnover NUMERIC(18,2),
		ProratedLoan NUMERIC(18,2)
	)

	DECLARE @mpId INT,
		@mpName NVARCHAR(255),
		@numOfNotFinishedMps INT,
		@numOfFinishedMps INT,
		@numOfFinishedCustomers INT,
		@sumOfScore INT,
		@avgScore NUMERIC(15, 2),
		@currentMarketPlaceId INT,
		@latestAggr DATETIME,
		@analysisFuncId INT,
		@AggrVal FLOAT,
		@TotalTurnoverNotFinished NUMERIC(18,2),
		@TotalTurnoverNotFinishedCounter INT,
		@avgTurnOverNotFinished NUMERIC(18,2),
		@TotalTurnoverFinished NUMERIC(18,2),
		@TotalTurnoverFinishedCounter INT,
		@avgTurnOverFinished NUMERIC(18,2),
		@numOfMaleFinishedMps INT,
		@malePercent NUMERIC(5,2),
		@avgAge NUMERIC(18,2),
		@CustomerId INT,
		@TmpTotalTurnover NUMERIC(18,2),
		@loanAmount NUMERIC(18,2),
		@MarketPlaceTypeId INT,
		@TotalTurnover NUMERIC(18,2),
		@ProratedLoan NUMERIC(18,2),
		@numOfShops INT

	DECLARE cur CURSOR FOR
		SELECT
			Id,
			Name
		FROM
			MP_MarketplaceType
		WHERE
			Name != 'PayPoint'

	OPEN cur

	FETCH NEXT FROM cur INTO @mpId, @mpName

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT
			Id
		INTO
			#NotFinishedCustomers
		FROM
			Customer
		WHERE
			@DateStart <= GreetingMailSentDate AND GreetingMailSentDate < @DateEnd
			AND
			WizardStep != 4
			AND
			Customer.IsTest = 0

		SELECT
			Id,
			Gender,
			DATEDIFF(year, DateOfBirth, GETDATE()) AS Age
		INTO
			#FinishedCustomers
		FROM
			Customer
		WHERE
			Customer.IsTest = 0
			AND
			@DateStart <= GreetingMailSentDate AND GreetingMailSentDate < @DateEnd
			AND
			WizardStep = 4

		DELETE FROM
			#NotFinishedCustomers
		WHERE
			Id NOT IN (
				SELECT DISTINCT CustomerId
				FROM MP_CustomerMarketPlace
				INNER JOIN #NotFinishedCustomers ON CustomerId = #NotFinishedCustomers.Id
				WHERE MarketPlaceId = @mpId
			)

		DELETE FROM
			#FinishedCustomers
		WHERE
			Id NOT IN (
				SELECT DISTINCT CustomerId
				FROM MP_CustomerMarketPlace
				INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id
				WHERE MarketPlaceId = @mpId
			)

		SET @numOfNotFinishedMps = 0
		SET @numOfFinishedMps = 0
		SET @TotalTurnoverNotFinished = 0
		SET @TotalTurnoverNotFinishedCounter = 0
		SET @TotalTurnoverFinished = 0
		SET @TotalTurnoverFinishedCounter = 0

		SELECT
			@numOfNotFinishedMps = COUNT(MarketPlaceId)
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #NotFinishedCustomers ON CustomerId = #NotFinishedCustomers.Id
		WHERE
			MarketPlaceId = @mpId

		SELECT
			@numOfFinishedMps = COUNT(MarketPlaceId)
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id
		WHERE
			MarketPlaceId = @mpId

		SELECT
			@numOfMaleFinishedMps = COUNT(MarketPlaceId)
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id AND Gender = 'M'
		WHERE
			MarketPlaceId = @mpId

		SET @avgAge = 0

		IF @numOfFinishedMps != 0
			SELECT
				@avgAge = AVG(Age)
			FROM
				#FinishedCustomers

		SELECT
			CustomerId,
			ExperianScore
		INTO
			#ExperianScores
		FROM
			(
				SELECT
					CustomerId,
					ExperianScore,
					ROW_NUMBER() OVER(PARTITION BY CustomerId ORDER BY Id DESC) AS rn
				FROM
					MP_ExperianDataCache
			) as T
		WHERE
			rn = 1
			AND
			CustomerId IS NOT NULL
			AND
			ExperianScore != 0
			AND
			CustomerId IN (
				SELECT DISTINCT CustomerId
				FROM MP_CustomerMarketPlace
				INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id
				WHERE MarketPlaceId = @mpId
			)

		SELECT
			@numOfFinishedCustomers = COUNT(1)
		FROM
			#ExperianScores

		SELECT
			@sumOfScore = SUM(ExperianScore)
		FROM
			#ExperianScores

		DROP TABLE #ExperianScores

		IF @sumOfScore IS NULL
			SET @avgScore = 0
		ELSE
			SET @avgScore = CONVERT(NUMERIC(15, 2), @sumOfScore) / CONVERT(NUMERIC(15, 2), @numOfFinishedCustomers)

		SELECT
			MP_CustomerMarketPlace.Id
		INTO
			#NotFinishedMarketPlaces
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #NotFinishedCustomers ON CustomerId = #NotFinishedCustomers.Id
		WHERE
			MarketPlaceId = @mpId

		SELECT
			MP_CustomerMarketPlace.Id,
			CustomerId
		INTO
			#FinishedMarketPlaces
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id
		WHERE
			MarketPlaceId = @mpId

		SELECT
			@analysisFuncId = Id
		FROM
			MP_AnalyisisFunction
		WHERE
			MarketPlaceId = @mpId
			AND
			(
				(@mpName = 'Pay pal' AND Name = 'TotalNetInPayments')
				OR
				(@mpName = 'PayPoint' AND Name = 'SumOfAuthorisedOrders')
				OR
				(@mpName NOT IN ('Pay pal', 'PayPoint') AND Name = 'TotalSumOfOrders')
			)

		IF @analysisFuncId IS NOT NULL
		BEGIN
			DECLARE marketCursor CURSOR FOR
				SELECT Id FROM #NotFinishedMarketPlaces

			OPEN marketCursor

			FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId

			WHILE @@FETCH_STATUS = 0
			BEGIN
				SELECT
					@latestAggr = MAX(Updated)
				FROM
					MP_AnalyisisFunctionValues
				WHERE
					AnalyisisFunctionId = @analysisFuncId
					AND
					CustomerMarketPlaceId = @currentMarketPlaceId

				IF @latestAggr IS NOT NULL
				BEGIN
					SELECT TOP 1
						@AggrVal = ValueFloat
					FROM
						MP_AnalyisisFunctionValues
					WHERE
						AnalyisisFunctionId = @analysisFuncId
						AND
						CustomerMarketPlaceId = @currentMarketPlaceId
						AND
						Updated = @latestAggr
						AND
						AnalysisFunctionTimePeriodId < 5
					ORDER BY
						AnalysisFunctionTimePeriodId DESC

					SET @TotalTurnoverNotFinished = @TotalTurnoverNotFinished + @AggrVal

					SET @TotalTurnoverNotFinishedCounter = @TotalTurnoverNotFinishedCounter + 1
				END

				FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId
			END

			CLOSE marketCursor
			DEALLOCATE marketCursor

			DECLARE marketCursor CURSOR FOR
				SELECT Id, CustomerId FROM #FinishedMarketPlaces

			OPEN marketCursor

			FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId, @CustomerId

			WHILE @@FETCH_STATUS = 0
			BEGIN
				SELECT
					@latestAggr = Max(Updated)
				FROM
					MP_AnalyisisFunctionValues
				WHERE
					AnalyisisFunctionId = @analysisFuncId
					AND
					CustomerMarketPlaceId = @currentMarketPlaceId

				IF @latestAggr IS NOT NULL
				BEGIN
					SELECT TOP 1
						@AggrVal = ValueFloat
					FROM
						MP_AnalyisisFunctionValues
					WHERE
						AnalyisisFunctionId = @analysisFuncId
						AND
						CustomerMarketPlaceId = @currentMarketPlaceId
						AND
						Updated = @latestAggr
						AND
						AnalysisFunctionTimePeriodId < 5
					ORDER BY
						AnalysisFunctionTimePeriodId DESC

					SET @TotalTurnoverFinished = @TotalTurnoverFinished + @AggrVal

					SET @TotalTurnoverFinishedCounter = @TotalTurnoverFinishedCounter + 1

					IF NOT EXISTS (SELECT 1 FROM #tmp2 WHERE CustomerId = @CustomerId)
						INSERT INTO #tmp2 VALUES (@CustomerId, @mpId, 1, 0, 0, 0)

					IF NOT EXISTS (SELECT 1 FROM #tmp2 WHERE CustomerId = @CustomerId AND #tmp2.MarketPlaceTypeId = @mpId)
					BEGIN
						INSERT INTO #tmp2
						SELECT TOP 1
							@CustomerId,
							@mpId,
							1,
							0,
							TotalTurnover,
							0
						FROM
							#tmp2
						WHERE
							CustomerId = @CustomerId
					END

					UPDATE #tmp2 SET
						TurnoverForType = TurnoverForType + @AggrVal
					WHERE
						CustomerId = @CustomerId
						AND
						MarketPlaceTypeId = @mpId

					UPDATE #tmp2 SET
						TotalTurnover = TotalTurnover + @AggrVal
					WHERE
						CustomerId = @CustomerId
				END

				FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId, @CustomerId
			END

			CLOSE marketCursor
			DEALLOCATE marketCursor
		END

		SET @avgTurnOverNotFinished = 0
		IF @TotalTurnoverNotFinishedCounter != 0
			SET @avgTurnOverNotFinished = @TotalTurnoverNotFinished / @TotalTurnoverNotFinishedCounter

		SET @avgTurnOverFinished = 0
		IF @TotalTurnoverFinishedCounter != 0
			SET @avgTurnOverFinished = @TotalTurnoverFinished / @TotalTurnoverFinishedCounter

		IF @numOfFinishedMps != 0
			SET @malePercent = @numOfMaleFinishedMps * 100.0 / @numOfFinishedMps
		ELSE
			SET @malePercent = 0

		INSERT INTO #tmp VALUES (
			@mpId, @numOfNotFinishedMps, @avgTurnOverNotFinished,
			@numOfFinishedMps, @avgTurnOverFinished, @avgScore,
			@malePercent, @avgAge, 0.0,
			0.0, 0
		)

		DROP TABLE #NotFinishedCustomers
		DROP TABLE #FinishedCustomers
		DROP TABLE #NotFinishedMarketPlaces
		DROP TABLE #FinishedMarketPlaces

		FETCH NEXT FROM cur INTO @mpId, @mpName
	END

	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR
		SELECT DISTINCT CustomerId FROM #tmp2

	OPEN cur

	FETCH NEXT FROM cur INTO @CustomerId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF EXISTS (SELECT 1 FROM DecisionHistory WHERE CustomerId = @CustomerId AND Action = 'Approve')
		BEGIN
			DECLARE typeCur CURSOR FOR
				SELECT MarketPlaceTypeId
				FROM #tmp2
				WHERE CustomerId = @CustomerId

			OPEN typeCur

			FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId

			WHILE @@FETCH_STATUS = 0
			BEGIN
				UPDATE #tmp SET
					NumOfShopsApproved = NumOfShopsApproved + 1
				WHERE
					MarketPlaceId = @MarketPlaceTypeId

				FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId
			END

			CLOSE typeCur
			DEALLOCATE typeCur
		END

		FETCH NEXT FROM cur INTO @CustomerId
	END

	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR
		SELECT DISTINCT CustomerId FROM #tmp2

	OPEN cur

	FETCH NEXT FROM cur INTO @CustomerId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @loanAmount = ISNULL(Sum(LoanAmount), 0) FROM Loan WHERE CustomerId=@CustomerId

		DECLARE typeCur CURSOR FOR
			SELECT MarketPlaceTypeId
			FROM #tmp2
			WHERE CustomerId = @CustomerId

		OPEN typeCur

		FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId

		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT
				@TotalTurnover = TotalTurnover
			FROM
				#tmp2
			WHERE
				CustomerId = @CustomerId
				AND
				MarketPlaceTypeId = @MarketPlaceTypeId

			IF @TotalTurnover != 0
				UPDATE #tmp2 SET
					ProratedLoan = @loanAmount * TurnoverForType / @TotalTurnover
				WHERE
					CustomerId = @CustomerId
					AND
					MarketPlaceTypeId = @MarketPlaceTypeId

			FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId
		END

		CLOSE typeCur
		DEALLOCATE typeCur

		FETCH NEXT FROM cur INTO @CustomerId
	END

	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR
		SELECT
			MarketPlaceTypeId,
			SUM(ProratedLoan)
		FROM
			#tmp2
		GROUP BY
			MarketPlaceTypeId

	OPEN cur

	FETCH NEXT FROM cur INTO @MarketPlaceTypeId, @ProratedLoan

	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE #tmp SET
			AvgAmountApproved = CASE NumOfShopsApproved
				 WHEN 0 THEN 0
				 ELSE @ProratedLoan / NumOfShopsApproved
			END
		WHERE
			MarketPlaceId = @MarketPlaceTypeId

		FETCH NEXT FROM cur INTO @MarketPlaceTypeId, @ProratedLoan
	END

	CLOSE cur
	DEALLOCATE cur

	DROP TABLE #tmp2

	SELECT
		Name,
		CASE
			WHEN NumOfShopsDidntFinish = 0 THEN NULL
			ELSE NumOfShopsDidntFinish
		END AS NumOfShopsDidntFinish,
		CASE
			WHEN AvgTurnoverDidntFinish = 0 THEN NULL
			ELSE CONVERT(INT, AvgTurnoverDidntFinish)
		END AS AvgTurnoverDidntFinish,
		CASE
			WHEN NumOfShopsFinish = 0 THEN NULL
			ELSE NumOfShopsFinish
		END AS NumOfShopsFinish,
		CASE
			WHEN AvgTurnoverFinish = 0 THEN NULL
			ELSE CONVERT(INT, AvgTurnoverFinish)
		END AS AvgTurnoverFinish,
		CASE
			WHEN AvgScore = 0 THEN NULL
			ELSE CONVERT(INT, AvgScore)
		END AS AvgScore,
		CASE
			WHEN PercentMen = 0 THEN NULL
			ELSE CONVERT(INT, PercentMen)
		END AS PercentMen,
		CASE
			WHEN AvgAge = 0 THEN NULL
			ELSE CONVERT(INT, AvgAge)
		END AS AvgAge,
		CASE
			WHEN NumOfShopsFinish IS NULL OR NumOfShopsFinish = 0 THEN NULL
			ELSE CASE
				WHEN NumOfShopsApproved = 0 THEN NULL
				ELSE CONVERT(INT, NumOfShopsApproved * 100.0 / NumOfShopsFinish)
			END
		END AS PercentApproved,
		CASE
			WHEN AvgAmountApproved = 0 THEN NULL
			ELSE CONVERT(INT, AvgAmountApproved)
		END AS AvgAmountApproved
	FROM
		#tmp
		INNER JOIN MP_MarketplaceType ON  #tmp.MarketPlaceId = MP_MarketplaceType.Id

	DROP TABLE #tmp
END
GO
