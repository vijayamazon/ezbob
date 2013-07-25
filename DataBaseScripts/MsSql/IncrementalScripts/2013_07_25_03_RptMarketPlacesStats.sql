IF OBJECT_ID('RptMarketPlacesStats') IS NOT NULL
	DROP PROCEDURE RptMarketPlacesStats
GO

CREATE PROCEDURE RptMarketPlacesStats
	@DateStart DATETIME,
	@DateEnd DATETIME
AS
BEGIN
	CREATE TABLE #tmp
	(
		MarketPlaceId INT,
		NumOfShopsDidntFinish INT,
		AvgTurnoverDidntFinish NUMERIC(18,2),
		NumOfShopsFinish INT,
		AvgTurnoverFinish NUMERIC(18,2),
		AvgScore NUMERIC(5,2),
		PercentMen NUMERIC(5,2),
		AvgAge NUMERIC(5,2),
		PercentApproved NUMERIC(5,2),
		AvgAmountApproved NUMERIC(10,2),
		NumOfShopsApproved INT
	)

	CREATE TABLE #tmp2
	(
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
			@avgScore NUMERIC(5,2),
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
			
	DECLARE cur CURSOR FOR SELECT Id, Name FROM MP_MarketplaceType
	OPEN cur
	FETCH NEXT FROM cur INTO @mpId, @mpName
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT Id INTO #NotFinishedCustomers FROM Customer WHERE GreetingMailSentDate >= @DateStart AND GreetingMailSentDate < @DateEnd AND WizardStep!=4
		SELECT Id, Gender, DATEDIFF(hour,DateOfBirth,GETDATE())/8766.0 AS Age INTO #FinishedCustomers FROM Customer WHERE GreetingMailSentDate >= @DateStart AND GreetingMailSentDate < @DateEnd AND WizardStep=4

		DELETE FROM #NotFinishedCustomers WHERE Id NOT IN (SELECT DISTINCT CustomerId FROM MP_CustomerMarketPlace WHERE MarketPlaceId=@mpId AND CustomerId IN (SELECT Id FROM #NotFinishedCustomers) )
		DELETE FROM #FinishedCustomers WHERE Id NOT IN (SELECT DISTINCT CustomerId FROM MP_CustomerMarketPlace WHERE MarketPlaceId=@mpId AND CustomerId IN (SELECT Id FROM #FinishedCustomers) )
	 
		SET @numOfNotFinishedMps = 0
		SET @numOfFinishedMps = 0	 
		SET @TotalTurnoverNotFinished = 0
		SET @TotalTurnoverNotFinishedCounter = 0
		SET @TotalTurnoverFinished = 0
		SET @TotalTurnoverFinishedCounter = 0	
		SELECT @numOfNotFinishedMps = count(MarketPlaceId) FROM MP_CustomerMarketPlace WHERE MarketPlaceId=@mpId AND CustomerId IN (SELECT Id FROM #NotFinishedCustomers) 
		SELECT @numOfFinishedMps = count(MarketPlaceId) FROM MP_CustomerMarketPlace WHERE MarketPlaceId=@mpId AND CustomerId IN (SELECT Id FROM #FinishedCustomers) 
		SELECT @numOfMaleFinishedMps = count(MarketPlaceId) FROM MP_CustomerMarketPlace WHERE MarketPlaceId=@mpId AND CustomerId IN (SELECT Id FROM #FinishedCustomers WHERE Gender='M') 

		SET @avgAge = 0
		IF @numOfFinishedMps != 0
		BEGIN
			SELECT @avgAge = sum(Age) FROM #FinishedCustomers
			SELECT @avgAge = @avgAge/count(1) FROM #FinishedCustomers
		END

		SELECT CustomerId, ExperianScore INTO #ExperianScores 
		FROM (SELECT CustomerId, ExperianScore, row_number() OVER(PARTITION BY CustomerId ORDER BY Id DESC) AS rn FROM MP_ExperianDataCache) as T
		WHERE rn = 1 AND 
			CustomerId IS NOT NULL AND 
			ExperianScore != 0 AND 
			CustomerId IN 
				(SELECT DISTINCT CustomerId FROM MP_CustomerMarketPlace 
				 WHERE MarketPlaceId=@mpId AND 
				 CustomerId IN 
				 (SELECT Id FROM Customer 
				  WHERE GreetingMailSentDate >= @DateStart AND 
				  GreetingMailSentDate < @DateEnd AND 
				  WizardStep=4))
		
		SELECT @numOfFinishedCustomers = count(1) FROM #ExperianScores
		SELECT @sumOfScore = sum(ExperianScore) FROM #ExperianScores
		DROP TABLE #ExperianScores
		
		IF (@sumOfScore IS NULL)
		BEGIN
			SET @avgScore = 0
		END
		ELSE
		BEGIN
			SET @avgScore = (@sumOfScore * 1.0) / @numOfFinishedCustomers
		END
					
		SELECT Id INTO #NotFinishedMarketPlaces FROM MP_CustomerMarketPlace WHERE MarketPlaceId=@mpId AND CustomerId IN (SELECT Id FROM #NotFinishedCustomers) 
		SELECT Id, CustomerId INTO #FinishedMarketPlaces FROM MP_CustomerMarketPlace WHERE MarketPlaceId=@mpId AND CustomerId IN (SELECT Id FROM #FinishedCustomers) 
					
		IF @mpName = 'PayPal'
		BEGIN
			SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpId AND Name='TotalNetInPayments'
		END
		ELSE
		BEGIN
			IF @mpName = 'PayPoint'
			BEGIN
				SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpId AND Name='SumOfAuthorisedOrders'
			END
			ELSE
			BEGIN
				SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpId AND Name='TotalSumOfOrders'
			END
		END

		IF @analysisFuncId IS NOT NULL
		BEGIN
			DECLARE marketCursor CURSOR FOR SELECT Id FROM #NotFinishedMarketPlaces			
			OPEN marketCursor			
			FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId			
			WHILE @@FETCH_STATUS = 0
			BEGIN
				SELECT @latestAggr = Max(Updated) FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@currentMarketPlaceId
				IF @latestAggr IS NOT NULL
				BEGIN
					SELECT TOP 1 ValueFloat INTO #MaxAggrValueNotFinished FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@currentMarketPlaceId AND Updated = @latestAggr AND AnalysisFunctionTimePeriodId < 5 ORDER BY AnalysisFunctionTimePeriodId DESC
					
					SELECT @AggrVal = ValueFloat FROM #MaxAggrValueNotFinished
					SET @TotalTurnoverNotFinished = @TotalTurnoverNotFinished + @AggrVal
					SET @TotalTurnoverNotFinishedCounter = @TotalTurnoverNotFinishedCounter + 1
					
					DROP TABLE #MaxAggrValueNotFinished
				END
				FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId
			END			
			CLOSE marketCursor
			DEALLOCATE marketCursor		
			
			DECLARE marketCursor CURSOR FOR SELECT Id, CustomerId FROM #FinishedMarketPlaces			
			OPEN marketCursor			
			FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId, @CustomerId			
			WHILE @@FETCH_STATUS = 0
			BEGIN
				SELECT @latestAggr = Max(Updated) FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@currentMarketPlaceId
				IF @latestAggr IS NOT NULL
				BEGIN
					SELECT TOP 1 ValueFloat INTO #MaxAggrValueFinished FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@currentMarketPlaceId AND Updated = @latestAggr AND AnalysisFunctionTimePeriodId < 5 ORDER BY AnalysisFunctionTimePeriodId DESC
					
					SELECT @AggrVal = ValueFloat FROM #MaxAggrValueFinished
					SET @TotalTurnoverFinished = @TotalTurnoverFinished + @AggrVal
					SET @TotalTurnoverFinishedCounter = @TotalTurnoverFinishedCounter + 1
					
					IF NOT EXISTS (SELECT 1 FROM #tmp2 WHERE CustomerId = @CustomerId)
					BEGIN
						INSERT INTO #tmp2 VALUES (@CustomerId, @mpId, 0, 0, 0, 0)					
					END
					
					IF NOT EXISTS (SELECT 1 FROM #tmp2 WHERE CustomerId = @CustomerId AND #tmp2.MarketPlaceTypeId=@mpId)
					BEGIN
						SELECT TOP 1 TotalTurnover INTO #tmpTotalTurnover FROM #tmp2 WHERE CustomerId = @CustomerId
						
						SELECT @TmpTotalTurnover = TotalTurnover FROM #tmpTotalTurnover
						INSERT INTO #tmp2 VALUES (@CustomerId, @mpId, 1, 0, @TmpTotalTurnover, 0)
						
						DROP TABLE #tmpTotalTurnover
					END
					
					UPDATE #tmp2 SET TurnoverForType = TurnoverForType + @AggrVal WHERE CustomerId = @CustomerId AND MarketPlaceTypeId=@mpId
					UPDATE #tmp2 SET TotalTurnover = TotalTurnover + @AggrVal WHERE CustomerId = @CustomerId
										
					DROP TABLE #MaxAggrValueFinished
				END
				FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId, @CustomerId
			END			
			CLOSE marketCursor
			DEALLOCATE marketCursor
		END		
		
		SET @avgTurnOverNotFinished = 0
		IF @TotalTurnoverNotFinishedCounter != 0
		BEGIN
			SET @avgTurnOverNotFinished = @TotalTurnoverNotFinished / @TotalTurnoverNotFinishedCounter
		END
		
		SET @avgTurnOverFinished = 0
		IF @TotalTurnoverFinishedCounter != 0
		BEGIN
			SET @avgTurnOverFinished = @TotalTurnoverFinished / @TotalTurnoverFinishedCounter
		END
		
		IF @numOfFinishedMps != 0
		BEGIN
			SET @malePercent = @numOfMaleFinishedMps * 100.0 / @numOfFinishedMps
		END
		ELSE
		BEGIN
			SET @malePercent = 0
		END			

		INSERT INTO #tmp VALUES (@mpId, @numOfNotFinishedMps, @avgTurnOverNotFinished, @numOfFinishedMps, @avgTurnOverFinished, @avgScore, @malePercent, @avgAge, 0.0, 0.0, 0)
				
		DROP TABLE #NotFinishedCustomers
		DROP TABLE #FinishedCustomers
		DROP TABLE #NotFinishedMarketPlaces
		DROP TABLE #FinishedMarketPlaces
		
		FETCH NEXT FROM cur INTO @mpId, @mpName
	END
	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR SELECT DISTINCT CustomerId FROM #tmp2
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN		
		SELECT @loanAmount = Sum(LoanAmount) FROM Loan WHERE CustomerId=@CustomerId
		IF @loanAmount IS NULL 
			SET @loanAmount = 0
		ELSE
		BEGIN		
			DECLARE numOfShopsCur CURSOR FOR 
				SELECT MarketPlaceId, count(MarketPlaceId) FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId GROUP BY MarketPlaceId
			OPEN numOfShopsCur			
			FETCH NEXT FROM numOfShopsCur INTO @MarketPlaceTypeId, @numOfShops			
			WHILE @@FETCH_STATUS = 0
			BEGIN		
				UPDATE #tmp SET NumOfShopsApproved = NumOfShopsApproved + @numOfShops WHERE MarketPlaceId = @MarketPlaceTypeId
				FETCH NEXT FROM numOfShopsCur INTO @MarketPlaceTypeId, @numOfShops		
			END					
			CLOSE numOfShopsCur
			DEALLOCATE numOfShopsCur
		END
		
		DECLARE typeCur CURSOR FOR SELECT MarketPlaceTypeId FROM #tmp2 WHERE CustomerId=@CustomerId
		OPEN typeCur		
		FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @TotalTurnover = TotalTurnover FROM #tmp2 WHERE CustomerId=@CustomerId AND MarketPlaceTypeId = @MarketPlaceTypeId		
			IF @TotalTurnover != 0
				UPDATE #tmp2 SET ProratedLoan = @loanAmount * TurnoverForType / @TotalTurnover WHERE CustomerId=@CustomerId AND MarketPlaceTypeId = @MarketPlaceTypeId
				
			FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId
		END		
		CLOSE typeCur
		DEALLOCATE typeCur

		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR SELECT MarketPlaceTypeId, sum (ProratedLoan) FROM #tmp2 GROUP BY MarketPlaceTypeId
	OPEN cur
	FETCH NEXT FROM cur INTO @MarketPlaceTypeId, @ProratedLoan
	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE #tmp SET AvgAmountApproved = @ProratedLoan / NumOfShopsFinish WHERE MarketPlaceId = @MarketPlaceTypeId
		FETCH NEXT FROM cur INTO @MarketPlaceTypeId, @ProratedLoan
	END
	CLOSE cur
	DEALLOCATE cur
	 
	DROP TABLE #tmp2
	
	SELECT 
		Name, 
		CASE WHEN NumOfShopsDidntFinish=0 THEN NULL ELSE NumOfShopsDidntFinish END AS NumOfShopsDidntFinish, 
		CASE WHEN AvgTurnoverDidntFinish=0 THEN NULL ELSE CONVERT(INT, AvgTurnoverDidntFinish) END AS AvgTurnoverDidntFinish, 
		CASE WHEN NumOfShopsFinish=0 THEN NULL ELSE NumOfShopsFinish END AS NumOfShopsFinish, 
		CASE WHEN AvgTurnoverFinish=0 THEN NULL ELSE CONVERT(INT, AvgTurnoverFinish) END AS AvgTurnoverFinish, 
		CASE WHEN AvgScore=0 THEN NULL ELSE CONVERT(INT, AvgScore) END AS AvgScore, 
		CASE WHEN PercentMen=0 THEN NULL ELSE CONVERT(INT, PercentMen) END AS PercentMen,	
		CASE WHEN AvgAge=0 THEN NULL ELSE CONVERT(INT, AvgAge) END AS AvgAge,	
		CASE WHEN NumOfShopsFinish IS NULL OR NumOfShopsFinish=0 THEN NULL ELSE CASE WHEN NumOfShopsApproved=0 THEN NULL ELSE CONVERT(INT, NumOfShopsApproved * 100.0 / NumOfShopsFinish) END END AS PercentApproved, 
		CASE WHEN AvgAmountApproved=0 THEN NULL ELSE CONVERT(INT, AvgAmountApproved) END AS AvgAmountApproved 
	FROM #tmp, MP_MarketplaceType WHERE MP_MarketplaceType.Id = #tmp.MarketPlaceId

	DROP TABLE #tmp
END
GO


IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type='RPT_MARKETPLACES_STATS')
BEGIN
	INSERT INTO ReportScheduler	VALUES
	(
		'RPT_MARKETPLACES_STATS'
		, 'MarketPlaces Stats'
		, 'RptMarketPlacesStats'
		, 0
		, 0
		, 0
		, 'Name, # Not finished wizard, Average turnover for not finished wizard, # Finished wizard, Average turnover for finished wizard, Average score, % of Men,	Average age, % Approved, Average amount approved'
		, 'Name, NumOfShopsDidntFinish, AvgTurnoverDidntFinish, NumOfShopsFinish, AvgTurnoverFinish, AvgScore, PercentMen,	AvgAge,	PercentApproved, AvgAmountApproved'
		, 'yulys@ezbob.com,nimrodk@ezbob.com'
		, 0
	)
	
	DECLARE @id INT,
			@reportId INT
			
	SELECT @reportId = Id FROM ReportScheduler WHERE Type = 'RPT_MARKETPLACES_STATS'
	
	DECLARE cur CURSOR FOR SELECT Id FROM ReportUsers WHERE Name IN ('yulys', 'nimrodk')
	OPEN cur
	FETCH NEXT FROM cur INTO @id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ReportsUsersMap WHERE UserID = @id AND ReportID = @reportId)
			INSERT INTO ReportsUsersMap VALUES (@id, @reportId)
	
		FETCH NEXT FROM cur INTO @id
	END
	CLOSE cur
	DEALLOCATE cur
END
GO





