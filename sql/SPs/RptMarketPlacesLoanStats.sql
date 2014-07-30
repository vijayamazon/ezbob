IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptMarketPlacesLoanStats]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptMarketPlacesLoanStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptMarketPlacesLoanStats] 
	(@DateStart DATETIME,
	@DateEnd DATETIME)
AS
BEGIN
	CREATE TABLE #ApprovedCustomers
	(
		CustomerId INT,
		Gender CHAR(1),
		Age NUMERIC(5,2),
		ApprovedLoan INT,
		LoanedAmount INT,
		ExperianScore INT
	)

	CREATE TABLE #Turnovers
	(
		CustomerId INT,
		MarketPlaceTypeId INT,
		MarketPlaceId INT,
		Turnover NUMERIC(18,2),
		ProratedLoanAmount NUMERIC(18,2),
		ProratedApprovedAmount NUMERIC(18,2),
		Gender CHAR(1),
		Age NUMERIC(5,2)
	)

	CREATE TABLE #tmp
	(
		MarketPlaceTypeId INT,
		NumOfShopsApproved INT,
		NumOfShopsLoaned INT,
		AvgTurnover NUMERIC(18,2),
		AvgLoanAmount NUMERIC(18,2),
		AvgApprovedAmount NUMERIC(18,2),
		AvgScore NUMERIC(5,2),
		PercentMen NUMERIC(5,2),
		AvgAge NUMERIC(5,2)
	)

	DECLARE @CustomerId INT,
			@ApprovedSum INT,
			@LoanedAmount INT,
			@MarketPlaceTypeId INT,
			@MarketPlaceId INT,
			@ExperianScore INT,
			@MarketPlaceName NVARCHAR(255),
			@AnalysisFuncId INT,
			@LatestAggregationTimeStamp DATETIME,
			@AggregateValue FLOAT,
			@NumOfApproved INT,
			@NumOfLoaned INT,
			@AvgTurnover NUMERIC(18,2),
			@AvgScore NUMERIC(18,2),
			@PercentMen NUMERIC(18,2),
			@AvgAge NUMERIC(18,2),
			@GenderHelper INT,
			@AgeHelper INT,
			@GenderHelper2 INT,
			@AvgLoaned NUMERIC(18,2),
			@AvgApproved NUMERIC(18,2),
			@TotalCustomerLoans NUMERIC(18,2),
			@TotalCustomerApproved NUMERIC(18,2),
			@TotalCustomerTurnover NUMERIC(18,2),
			@TurnoverForCurrentMp NUMERIC(18,2),
			@Age NUMERIC(18,2),
			@Gender CHAR(1)

	INSERT INTO #ApprovedCustomers 
	SELECT DISTINCT Customer.Id, Gender, DATEDIFF(hour,DateOfBirth,GETDATE())/8766.0 AS Age, 0,0,0 FROM Customer, DecisionHistory 
	WHERE 
		Customer.IsTest = 0 AND
		Customer.Id=DecisionHistory.CustomerId AND 
		Action='Approve' AND 
		GreetingMailSentDate >= @DateStart AND 
		GreetingMailSentDate < @DateEnd AND 
		WizardStep=4 

	DECLARE cur CURSOR FOR SELECT CustomerId FROM #ApprovedCustomers
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @ApprovedSum = sum(ManagerApprovedSum) FROM CashRequests WHERE IdCustomer=@CustomerId AND ManagerApprovedSum IS NOT NULL
		SELECT @LoanedAmount = sum(Loan.LoanAmount) FROM Loan WHERE CustomerId=@CustomerId
		SELECT @ExperianScore = (SELECT ExperianConsumerScore FROM Customer WHERE Id=@CustomerId)

		UPDATE #ApprovedCustomers SET ApprovedLoan = @ApprovedSum, LoanedAmount = @LoanedAmount, ExperianScore = @ExperianScore WHERE CustomerId=@CustomerId

		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR SELECT CustomerId, Gender, Age FROM #ApprovedCustomers
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId, @Gender, @Age
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE mpcur CURSOR FOR SELECT MP_MarketplaceType.Id, Name, MP_CustomerMarketPlace.Id FROM MP_MarketplaceType, MP_CustomerMarketPlace WHERE MP_MarketplaceType.Id = MarketPlaceId AND CustomerId = @CustomerId AND Name != 'PayPoint'
		OPEN mpcur
		FETCH NEXT FROM mpcur INTO @MarketPlaceTypeId, @MarketPlaceName, @MarketPlaceId
		WHILE @@FETCH_STATUS = 0
		BEGIN	
			IF @MarketPlaceName = 'Pay Pal'
			BEGIN
				SELECT @AnalysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@MarketPlaceTypeId AND Name='TotalNetInPayments'
			END
			ELSE
			BEGIN
				IF @MarketPlaceName = 'PayPoint'
				BEGIN
					SELECT @AnalysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@MarketPlaceTypeId AND Name='SumOfAuthorisedOrders'
				END
				ELSE
				BEGIN
					SELECT @AnalysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@MarketPlaceTypeId AND Name='TotalSumOfOrders'
				END
			END			
			
			IF @AnalysisFuncId IS NOT NULL
			BEGIN
				SELECT @LatestAggregationTimeStamp = Max(Updated) FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@AnalysisFuncId AND CustomerMarketPlaceId=@MarketPlaceId
				IF @LatestAggregationTimeStamp IS NOT NULL
				BEGIN
					SELECT TOP 1 ValueFloat INTO #MaxAggrValueFinished FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@AnalysisFuncId AND CustomerMarketPlaceId=@MarketPlaceId AND Updated = @LatestAggregationTimeStamp AND AnalysisFunctionTimePeriodId < 5 ORDER BY AnalysisFunctionTimePeriodId DESC
					
					SELECT @AggregateValue = ValueFloat FROM #MaxAggrValueFinished
					
					INSERT INTO #Turnovers VALUES (@CustomerId, @MarketPlaceTypeId, @MarketPlaceId, @AggregateValue, 0, 0, @Gender, @Age)
					
										
					DROP TABLE #MaxAggrValueFinished
				END
			END			
		
			FETCH NEXT FROM mpcur INTO @MarketPlaceTypeId, @MarketPlaceName, @MarketPlaceId
		END
		CLOSE mpcur
		DEALLOCATE mpcur

		FETCH NEXT FROM cur INTO @CustomerId, @Gender, @Age
	END
	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR SELECT DISTINCT CustomerId FROM #Turnovers
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @TotalCustomerLoans = sum(LoanedAmount) FROM #ApprovedCustomers WHERE CustomerId=@CustomerId AND LoanedAmount IS NOT NULL
		SELECT @TotalCustomerApproved = sum(ApprovedLoan) FROM #ApprovedCustomers WHERE CustomerId=@CustomerId AND ApprovedLoan IS NOT NULL
		SELECT @TotalCustomerTurnover = sum(Turnover) FROM #Turnovers WHERE CustomerId=@CustomerId
			
		DECLARE mpcur CURSOR FOR SELECT MarketPlaceId FROM #Turnovers WHERE CustomerId=@CustomerId
		OPEN mpcur
		FETCH NEXT FROM mpcur INTO @MarketPlaceId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @TurnoverForCurrentMp = Turnover FROM #Turnovers WHERE CustomerId=@CustomerId AND MarketPlaceId = @MarketPlaceId
			UPDATE #Turnovers SET 
				ProratedLoanAmount = @TurnoverForCurrentMp * @TotalCustomerLoans / @TotalCustomerTurnover, 
				ProratedApprovedAmount = @TurnoverForCurrentMp * @TotalCustomerApproved / @TotalCustomerTurnover
			WHERE CustomerId=@CustomerId AND MarketPlaceId = @MarketPlaceId

			FETCH NEXT FROM mpcur INTO @MarketPlaceId
		END
		CLOSE mpcur
		DEALLOCATE mpcur


		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur
	
	DECLARE mpcur CURSOR FOR SELECT Id FROM MP_MarketplaceType WHERE Name != 'PayPoint'
	OPEN mpcur
	FETCH NEXT FROM mpcur INTO @MarketPlaceTypeId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @NumOfApproved = count(1) FROM #Turnovers WHERE MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @NumOfLoaned = count(1) FROM #Turnovers, #ApprovedCustomers WHERE MarketPlaceTypeId = @MarketPlaceTypeId AND #Turnovers.CustomerId = #ApprovedCustomers.CustomerId AND #ApprovedCustomers.LoanedAmount IS NOT NULL
		
		SELECT @AvgTurnover = sum(Turnover)/count(1) FROM #Turnovers WHERE Turnover > 0 AND MarketPlaceTypeId = @MarketPlaceTypeId
		
		SELECT @AvgLoaned = sum(ProratedLoanAmount) / @NumOfLoaned FROM #Turnovers WHERE ProratedLoanAmount IS NOT NULL AND MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @AvgApproved = sum(ProratedApprovedAmount) / @NumOfApproved FROM #Turnovers WHERE ProratedApprovedAmount IS NOT NULL AND MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @AvgScore = sum(ExperianScore)/count(1) FROM #Turnovers, #ApprovedCustomers WHERE MarketPlaceTypeId = @MarketPlaceTypeId AND #Turnovers.CustomerId = #ApprovedCustomers.CustomerId AND #ApprovedCustomers.ExperianScore > 0
		
		SELECT @GenderHelper = count(1) FROM #Turnovers WHERE Gender = 'M' AND MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @GenderHelper2 = count(1) FROM #Turnovers WHERE MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @PercentMen = CASE WHEN @GenderHelper2 = 0 THEN 0 ELSE @GenderHelper * 100.0 / @GenderHelper2 END
			
		
		SELECT @AgeHelper = count(1) FROM #Turnovers WHERE MarketPlaceTypeId = @MarketPlaceTypeId
		IF @AgeHelper=0
		BEGIN
			SELECT @AvgAge = 0
		END
		ELSE
		BEGIN
			SELECT @AvgAge = sum(Age) / @AgeHelper FROM #Turnovers WHERE MarketPlaceTypeId = @MarketPlaceTypeId
		END
		
		INSERT INTO #tmp VALUES(@MarketPlaceTypeId, @NumOfApproved, @NumOfLoaned, @AvgTurnover, @AvgLoaned, @AvgApproved, @AvgScore, @PercentMen, @AvgAge)
	 
		FETCH NEXT FROM mpcur INTO @MarketPlaceTypeId
	END
	CLOSE mpcur
	DEALLOCATE mpcur

	SELECT 
		Name,
		CASE WHEN NumOfShopsApproved=0 THEN NULL ELSE NumOfShopsApproved END AS NumOfShopsApproved,
		CASE WHEN NumOfShopsLoaned=0 THEN NULL ELSE NumOfShopsLoaned END AS NumOfShopsLoaned,
		CONVERT(INT, AvgTurnover) AS AvgTurnover,
		CONVERT(INT, AvgLoanAmount) AS AvgLoanAmount,
		CONVERT(INT, AvgApprovedAmount) AS AvgApprovedAmount,
		CONVERT(INT, AvgScore) AS AvgScore,
		CASE WHEN PercentMen = 0 THEN NULL ELSE CONVERT(INT, PercentMen) END AS PercentMen,
		CASE WHEN AvgAge = 0 THEN NULL ELSE CONVERT(INT, AvgAge) END AS AvgAge,
		CASE WHEN NumOfShopsApproved = 0 THEN NULL ELSE CONVERT(INT, 100.0 * NumOfShopsLoaned / NumOfShopsApproved) END AS UtilizationByNum,
		CASE WHEN AvgTurnover=0 OR NumOfShopsApproved=0 THEN NULL ELSE CONVERT(INT, (100.0 * AvgLoanAmount * NumOfShopsLoaned) / (AvgApprovedAmount * NumOfShopsApproved)) END AS UtilizationByAmount	
	FROM #tmp, MP_MarketplaceType WHERE Id = MarketPlaceTypeId
	 
	DROP TABLE #ApprovedCustomers 
	DROP TABLE #Turnovers 
	DROP TABLE #tmp
END
GO
