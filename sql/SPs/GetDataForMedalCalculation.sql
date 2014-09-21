IF OBJECT_ID('GetDataForMedalCalculation') IS NULL
	EXECUTE('CREATE PROCEDURE GetDataForMedalCalculation AS SELECT 1')
GO

ALTER PROCEDURE GetDataForMedalCalculation
@CustomerId INT
AS
BEGIN
	DECLARE 
		@FirstRepaymentDate DATETIME, 
		@FirstRepaymentDatePassed BIT, 
		@OnTimeLoans INT, 
		@NumOfLatePayments INT, 
		@NumOfEarlyPayments INT,
		@BusinessScore INT,
		@ConsumerScore INT,
		@TangibleEquity DECIMAL(18,6),
		@BusinessSeniority DATETIME,
		@EzbobSeniority DATETIME,
		@MaritalStatus NVARCHAR(50),
		@HmrcId INT,
		@YodleeTotalAggrgationFuncId INT,
		@YodleeTurnover DECIMAL(18,6),
		@Threshold DECIMAL(18,6),
		@LoanScheduleId INT,
		@ScheduleDate DATETIME,
		@LoanId INT,
		@LastTransactionId INT,
		@LastTransactionDate DATETIME,
		@StatusAfterLastTransaction NVARCHAR(50),
		@LateDays INT,
		@NumOfHmrcMps INT,
		@TotalZooplaValue INT
		
	SET @Threshold = 2
	
	CREATE TABLE #LateLoans
	(
		LoanId INT
	)
	
	SELECT 
		@BusinessScore = Score, 
		@TangibleEquity = TangibleEquity, 
		@BusinessSeniority = IncorporationDate 
	FROM 
		CustomerAnalyticsCompany 
	WHERE 
		CustomerID = @CustomerId
	
	SELECT @ConsumerScore = MIN(ExperianConsumerScore)
	FROM
	(
		SELECT ExperianConsumerScore
		FROM Customer
		WHERE Id=@CustomerId
		UNION
		SELECT ExperianConsumerScore
		FROM Director
		WHERE CustomerId=@CustomerId AND ExperianConsumerScore IS NOT NULL
	) AS X
			
	SELECT 
		@EzbobSeniority = GreetingMailSentDate, 
		@MaritalStatus = MaritalStatus
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId

	SELECT @FirstRepaymentDatePassed = 0
	SELECT 
		@FirstRepaymentDate = MIN(LoanSchedule.Date) 
	FROM 
		LoanSchedule, 
		Loan 
	WHERE 
		Loan.Id = LoanSchedule.LoanId AND 
		Loan.CustomerId = @CustomerId

	IF @FirstRepaymentDate IS NOT NULL AND @FirstRepaymentDate < GETUTCDATE()
		SELECT @FirstRepaymentDatePassed = 1
	
	SELECT @OnTimeLoans = COUNT(1) FROM Loan WHERE CustomerId = @CustomerId
	
	DECLARE cur CURSOR FOR 
	SELECT 
		Id
	FROM 
		Loan
	WHERE 
		CustomerId = @CustomerId
		
	OPEN cur
	FETCH NEXT FROM cur INTO @LoanId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE cur2 CURSOR FOR 
		SELECT 
			Id,
			Date
		FROM 
			LoanSchedule
		WHERE 
			LoanId = @LoanId
		OPEN cur2
		FETCH NEXT FROM cur2 INTO @LoanScheduleId, @ScheduleDate
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF NOT EXISTS (SELECT 1 FROM #LateLoans WHERE LoanId = @LoanId)
			BEGIN
				SELECT 
					@LastTransactionId = Max(TransactionID) 
				FROM 
					LoanScheduleTransaction 
				WHERE 
					ScheduleID = @LoanScheduleId AND 
					ABS(PrincipalDelta) + ABS(FeesDelta) + ABS(InterestDelta) > @Threshold

				IF @LastTransactionId IS NOT NULL
				BEGIN
					SELECT @LastTransactionDate = PostDate FROM LoanTransaction WHERE Id = @LastTransactionId
					SELECT @StatusAfterLastTransaction = StatusAfter FROM LoanScheduleTransaction WHERE ScheduleID = @LoanScheduleId AND TransactionID = @LastTransactionId
				END

				IF @LastTransactionDate IS NOT NULL AND 
					(
						@StatusAfterLastTransaction = 'Paid' OR 
						@StatusAfterLastTransaction = 'PaidOnTime' OR 
						@StatusAfterLastTransaction = 'PaidEarly'
					)
					SELECT @LateDays = datediff(dd, @ScheduleDate, @LastTransactionDate)
				ELSE
					SELECT @LateDays = datediff(dd, @ScheduleDate, GETUTCDATE())
					
				IF @LateDays >= 7 
				BEGIN
					SET @OnTimeLoans = @OnTimeLoans - 1
					INSERT INTO #LateLoans VALUES (@LoanId)
				END
			END
			
			FETCH NEXT FROM cur2 INTO @LoanScheduleId, @ScheduleDate
		END
		CLOSE cur2
		DEALLOCATE cur2

		FETCH NEXT FROM cur INTO @LoanId
	END
	CLOSE cur
	DEALLOCATE cur
	
	DROP TABLE #LateLoans

	SELECT 
		@NumOfLatePayments = COUNT(LoanCharges.Id) 
	FROM 
		LoanCharges,
		ConfigurationVariables,  
		Loan
	WHERE 
		ConfigurationVariables.Name = 'LatePaymentCharge' AND
		ConfigurationVariables.Id = LoanCharges.ConfigurationVariableId AND
		Loan.Id = LoanCharges.LoanId AND
		Loan.CustomerId = @CustomerId

	SELECT 
		@NumOfEarlyPayments = COUNT(LoanSchedule.id) 
	FROM 
		LoanSchedule, 
		Loan
	WHERE 
		LoanSchedule.Status = 'PaidEarly' AND
		Loan.Id = LoanSchedule.LoanId AND
		Loan.CustomerId = @CustomerId
		
	SELECT
		@NumOfHmrcMps = COUNT(1)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'HMRC' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId
		
	SELECT TOP 1
		@HmrcId = MP_CustomerMarketplace.Id
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		CustomerId = @CustomerId AND
		MP_CustomerMarketplace.MarketPlaceId = MP_MarketplaceType.Id AND
		MP_MarketplaceType.Name = 'HMRC'
		
	IF @HmrcId IS NULL
		SELECT @HmrcId = 0
		
	SELECT @YodleeTotalAggrgationFuncId = MP_AnalyisisFunction.Id FROM MP_AnalyisisFunction, MP_MarketplaceType WHERE MP_AnalyisisFunction.MarketPlaceId=MP_MarketplaceType.Id AND MP_MarketplaceType.Name = 'Yodlee' AND MP_AnalyisisFunction.Name='TotalIncomeAnnualized'
	
	SELECT TOP 1 
		@YodleeTurnover = ValueFloat 
	FROM 
		MP_AnalyisisFunctionValues, 
		MP_CustomerMarketPlace 
	WHERE 
		MP_AnalyisisFunctionValues.CustomerMarketPlaceId = MP_CustomerMarketPlace.Id AND 
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND 
		AnalyisisFunctionId = @YodleeTotalAggrgationFuncId 
	ORDER BY 
		AnalysisFunctionTimePeriodId DESC
	
	SELECT 
		@TotalZooplaValue = SUM(CASE WHEN ZooplaEstimateValue != 0 THEN ZooplaEstimateValue ELSE AverageSoldPrice1Year END)
	FROM 
		Zoopla, 
		CustomerAddress 
	WHERE 
		CustomerId = @CustomerId AND 
		CustomerAddress.addressId = Zoopla.CustomerAddressId AND 
		CustomerAddress.IsOwnerAccordingToLandRegistry = 1	
				
	SELECT
		@FirstRepaymentDatePassed AS FirstRepaymentDatePassed, 
		@OnTimeLoans AS OnTimeLoans, 
		@NumOfLatePayments AS NumOfLatePayments, 
		@NumOfEarlyPayments AS NumOfEarlyPayments,
		@BusinessScore AS BusinessScore,
		@ConsumerScore AS ConsumerScore,
		@TangibleEquity AS TangibleEquity,
		@BusinessSeniority AS BusinessSeniority,
		@EzbobSeniority AS EzbobSeniority,
		@MaritalStatus AS MaritalStatus,
		@HmrcId AS HmrcId,
		@YodleeTurnover AS YodleeTurnover,
		@NumOfHmrcMps AS NumOfHmrcMps,
		@TotalZooplaValue AS TotalZooplaValue
END
GO
