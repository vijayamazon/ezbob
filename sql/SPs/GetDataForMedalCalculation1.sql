IF OBJECT_ID('GetDataForMedalCalculation1') IS NULL
	EXECUTE('CREATE PROCEDURE GetDataForMedalCalculation1 AS SELECT 1')
GO

ALTER PROCEDURE GetDataForMedalCalculation1
	(@CustomerId INT,
	 @CalculationTime DATETIME)
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
		@Threshold DECIMAL(18,6),
		@LoanScheduleId INT,
		@ScheduleDate DATETIME,
		@LoanId INT,
		@LastTransactionId INT,
		@LastTransactionDate DATETIME,
		@StatusAfterLastTransaction NVARCHAR(50),
		@LateDays INT,
		@NumOfHmrcMps INT,
		@TotalZooplaValue INT,
		@MpId INT,
		@LastUpdateTime DATETIME,
		@CurrentMpTurnoverValue FLOAT,
		@LoanStatus NVARCHAR(50)
		
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
		CustomerID = @CustomerId AND
		IsActive = 1
		
	IF @BusinessSeniority IS NULL
		SET @BusinessSeniority = @CalculationTime
	
	SELECT @ConsumerScore = MIN(ExperianConsumerScore)
	FROM
	(
		SELECT ExperianConsumerScore
		FROM Customer
		WHERE Id = @CustomerId AND ExperianConsumerScore IS NOT NULL
		UNION
		SELECT ExperianConsumerScore
		FROM Director
		WHERE CustomerId = @CustomerId AND ExperianConsumerScore IS NOT NULL
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

	IF @FirstRepaymentDate IS NOT NULL AND @FirstRepaymentDate < @CalculationTime
		SELECT @FirstRepaymentDatePassed = 1
	
	SELECT @OnTimeLoans = COUNT(1) FROM Loan WHERE CustomerId = @CustomerId AND Status = 'PaidOff'
	
	DECLARE cur CURSOR FOR 
	SELECT 
		Id,
		Status
	FROM 
		Loan
	WHERE 
		CustomerId = @CustomerId
		
	OPEN cur
	FETCH NEXT FROM cur INTO @LoanId, @LoanStatus
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
					SELECT @LateDays = datediff(dd, @ScheduleDate, @CalculationTime)
					
				IF @LateDays >= 7 
				BEGIN
					IF @LoanStatus = 'PaidOff'
						SET @OnTimeLoans = @OnTimeLoans - 1
						
					INSERT INTO #LateLoans VALUES (@LoanId)
				END
			END
			
			FETCH NEXT FROM cur2 INTO @LoanScheduleId, @ScheduleDate
		END
		CLOSE cur2
		DEALLOCATE cur2

		FETCH NEXT FROM cur INTO @LoanId, @LoanStatus
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
	
	SELECT 
		@TotalZooplaValue = SUM(CASE WHEN ZooplaEstimateValue IS NOT NULL AND ZooplaEstimateValue != 0 THEN ZooplaEstimateValue ELSE ISNULL(AverageSoldPrice1Year, 0) END)
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
		@NumOfHmrcMps AS NumOfHmrcMps,
		@TotalZooplaValue AS TotalZooplaValue
END
GO
