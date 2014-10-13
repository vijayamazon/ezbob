IF OBJECT_ID('GetDataForMedalCalculation2') IS NULL
	EXECUTE('CREATE PROCEDURE GetDataForMedalCalculation2 AS SELECT 1')
GO

ALTER PROCEDURE GetDataForMedalCalculation2
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
		@YodleeTotalAggrgationFuncId INT,
		@YodleeTurnover DECIMAL(18,6),
		@TypeOfBusiness NVARCHAR(50),
		@NumOfHmrcMps INT,
		@LastServiceLogId BIGINT,
		@Threshold DECIMAL(18,6),
		@LoanId INT,
		@LoanScheduleId INT,
		@LastTransactionId INT,
		@LastTransactionDate DATETIME,
		@StatusAfterLastTransaction NVARCHAR(50),
		@LateDays INT,
		@ScheduleDate DATETIME,
		@Ebida DECIMAL(18,6),
		@HmrcAnnualTurnover DECIMAL(18,6),
		@HmrcValueAdded DECIMAL(18,6),
		@BalanceOfMortgages INT,
		@FcfFactor NVARCHAR(MAX),
		@ActualLoanRepayments INT,
		@FoundSummary BIT,
		@TotalZooplaValue INT,
		@MpId INT,
		@LastUpdateTime DATETIME,
		@CurrentMpTurnoverValue FLOAT,
		@LoanStatus NVARCHAR(50),
		@EarliestHmrcLastUpdateDate DATETIME,
		@EarliestYodleeLastUpdateDate DATETIME
	
	SET @Threshold = 2 -- Hardcoded value. Used to avoid the entries in the LoanScheduleTransaction table that are there because of rounding mistakes
	
	SELECT @FcfFactor = Value FROM ConfigurationVariables WHERE Name = 'FCFFactor'
		
	SELECT
		@TypeOfBusiness = TypeOfBusiness,
		@EzbobSeniority = GreetingMailSentDate, 
		@MaritalStatus = MaritalStatus
	FROM
		Customer
	WHERE
		Customer.Id = @CustomerId
	
	SELECT
		@NumOfHmrcMps = COUNT(1),		
		@EarliestHmrcLastUpdateDate = MIN(UpdatingEnd)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'HMRC' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId
		
	SELECT
		@EarliestYodleeLastUpdateDate = MIN(UpdatingEnd)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'Yodlee' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId	
		
	SELECT 
		@BusinessScore = Score, 
		@TangibleEquity = TangibleEquity, 
		@BusinessSeniority = IncorporationDate,		
		@ActualLoanRepayments = CurrentBalanceSum
	FROM 
		CustomerAnalyticsCompany 
	WHERE 
		CustomerID = @CustomerId AND
		IsActive = 1
		
	IF @BusinessSeniority IS NULL
		SET @BusinessSeniority = @CalculationTime

	SELECT 
		@ConsumerScore = MIN(ExperianConsumerScore)
	FROM
		(
			SELECT ExperianConsumerScore
			FROM Customer
			WHERE Id = @CustomerId AND ExperianConsumerScore IS NOT NULL
			
			UNION
			
			SELECT ExperianConsumerScore
			FROM Director
			WHERE CustomerId = @CustomerId AND ExperianConsumerScore IS NOT NULL
		) AS ExperianConsumerScores
			
	SELECT 
		@FirstRepaymentDate = MIN(LoanSchedule.Date) 
	FROM 
		LoanSchedule, 
		Loan 
	WHERE 
		Loan.Id = LoanSchedule.LoanId AND 
		Loan.CustomerId = @CustomerId

	
	CREATE TABLE #LateLoans
	(
		LoanId INT
	)
	
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
	
	SELECT @Ebida = SUM(AnnualizedFreeCashFlow), @HmrcAnnualTurnover = SUM(AnnualizedTurnover), @HmrcValueAdded = SUM(AnnualizedValueAdded) FROM MP_VatReturnSummary WHERE CustomerId = @CustomerId AND IsActive = 1
		
	IF @Ebida IS NULL
		SELECT @FoundSummary = 0
	ELSE
		SELECT @FoundSummary = 1
	
	SELECT @YodleeTotalAggrgationFuncId = MP_AnalyisisFunction.Id FROM MP_AnalyisisFunction, MP_MarketplaceType WHERE MP_AnalyisisFunction.MarketPlaceId=MP_MarketplaceType.Id AND MP_MarketplaceType.Name = 'Yodlee' AND MP_AnalyisisFunction.Name='TotalIncomeAnnualized'
	SET @YodleeTurnover = 0
	
	DECLARE cur3 CURSOR FOR 
	SELECT 
		DISTINCT MP_CustomerMarketPlace.Id
	FROM 
		MP_AnalyisisFunctionValues, 
		MP_CustomerMarketPlace 
	WHERE 
		MP_AnalyisisFunctionValues.CustomerMarketPlaceId = MP_CustomerMarketPlace.Id AND 
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND 
		AnalyisisFunctionId = @YodleeTotalAggrgationFuncId
		
	OPEN cur3
	FETCH NEXT FROM cur3 INTO @MpId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT 
			@LastUpdateTime = MAX(Updated)
		FROM 
			MP_AnalyisisFunctionValues
		WHERE 
			MP_AnalyisisFunctionValues.CustomerMarketPlaceId = @MpId AND
			AnalyisisFunctionId = @YodleeTotalAggrgationFuncId 
	
		SELECT TOP 1
			@CurrentMpTurnoverValue = ValueFloat
		FROM 
			MP_AnalyisisFunctionValues
		WHERE 
			MP_AnalyisisFunctionValues.CustomerMarketPlaceId = @MpId AND 
			Updated = @LastUpdateTime AND 
			AnalyisisFunctionId = @YodleeTotalAggrgationFuncId AND
			AnalysisFunctionTimePeriodId < 5
		ORDER BY 
			AnalysisFunctionTimePeriodId DESC
	
		SET @YodleeTurnover = @YodleeTurnover + @CurrentMpTurnoverValue

		FETCH NEXT FROM cur3 INTO @MpId
	END
	CLOSE cur3
	DEALLOCATE cur3	
	
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
		@TotalZooplaValue = SUM(CASE WHEN ZooplaEstimateValue != 0 THEN ZooplaEstimateValue ELSE AverageSoldPrice1Year END)
	FROM 
		Zoopla, 
		CustomerAddress 
	WHERE 
		CustomerId = @CustomerId AND 
		CustomerAddress.addressId = Zoopla.CustomerAddressId AND 
		CustomerAddress.IsOwnerAccordingToLandRegistry = 1	
	
	SELECT 
		TOP 1 @LastServiceLogId = Id
	FROM
		MP_ServiceLog
	WHERE
		CustomerId = @CustomerId
		AND
		DirectorId IS NULL
		AND
		ServiceType = 'Consumer Request'
	ORDER BY
		Id DESC
		
	IF @LastServiceLogId IS NULL
	BEGIN
		SELECT TOP 1
		   @LastServiceLogId = l.Id
		FROM
		 Customer c 
		 INNER JOIN CustomerAddress a ON a.CustomerId = c.Id AND a.addressType=1
		 INNER JOIN MP_ServiceLog l on
		  l.Firstname = c.FirstName AND
		  l.Surname = c.Surname AND 
		  l.DateOfBirth = c.DateOfBirth AND
		  l.Postcode = a.Postcode AND
		  l.ServiceType = 'Consumer Request'
		  
		  WHERE
		   c.Id=@CustomerId
		  ORDER BY
		   l.InsertDate DESC,
		   l.Id DESC
	END

	SELECT 
		@BalanceOfMortgages = SUM(Balance) 
	FROM
		ExperianConsumerDataCais,
		ExperianConsumerData
	WHERE
		ServiceLogId = @LastServiceLogId AND
		AccountType IN ('03','16','25','30','31','32','33','34','35','69') AND -- Add '50' or change to name like '%mortgage%' 
		MatchTo = 1 AND
		AccountStatus <> 'S' AND
		ExperianConsumerData.Id = ExperianConsumerDataId	
	
	SELECT
		@NumOfHmrcMps AS NumOfHmrcMps,
		@TypeOfBusiness AS TypeOfBusiness, 
		@EzbobSeniority AS EzbobSeniority,
		@MaritalStatus AS MaritalStatus,
		@BusinessScore AS BusinessScore,
		@ConsumerScore AS ConsumerScore,
		@TangibleEquity AS TangibleEquity,
		@BusinessSeniority AS BusinessSeniority,		
		@FirstRepaymentDate AS FirstRepaymentDate, 
		@OnTimeLoans AS OnTimeLoans,
		@Ebida AS Ebida, 
		@HmrcAnnualTurnover AS HmrcAnnualTurnover,
		@HmrcValueAdded AS HmrcValueAdded,
		@YodleeTurnover AS YodleeTurnover,		 
		@NumOfLatePayments AS NumOfLatePayments, 
		@NumOfEarlyPayments AS NumOfEarlyPayments,
		@TotalZooplaValue AS TotalZooplaValue,
		@BalanceOfMortgages AS BalanceOfMortgages,
		@ActualLoanRepayments AS ActualLoanRepayments,
		@FcfFactor AS FcfFactor,
		@FoundSummary AS FoundSummary,
		@EarliestHmrcLastUpdateDate AS EarliestHmrcLastUpdateDate,
		@EarliestYodleeLastUpdateDate AS EarliestYodleeLastUpdateDate
END
GO
