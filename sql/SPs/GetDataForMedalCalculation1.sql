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
		@LoanStatus NVARCHAR(50),
		@EarliestHmrcLastUpdateDate DATETIME,
		@EarliestYodleeLastUpdateDate DATETIME,
		@NumberOfOnlineStores INT,
		@AmazonPositiveFeedbacks INT,
		@EbayPositiveFeedbacks INT,
		@NumOfPaypalTransactions INT,
		@TypeOfBusiness NVARCHAR(50),
		@RefNumber NVARCHAR(50),
		@FirstHmrcDate DATETIME,
		@FirstYodleeDate DATETIME
		
	SET @Threshold = 2
			
	SELECT 
		@EzbobSeniority = GreetingMailSentDate, 
		@MaritalStatus = MaritalStatus,
		@TypeOfBusiness = TypeOfBusiness
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId
	
	CREATE TABLE #LateLoans
	(
		LoanId INT
	)	
	
	SELECT
		@RefNumber = ExperianRefNum
	FROM
		Customer,
		Company
	WHERE
		Customer.Id = @CustomerId AND
		Customer.CompanyId = Company.Id
	
	IF @TypeOfBusiness = 'LLP' OR @TypeOfBusiness = 'Limited'
	BEGIN
		SELECT 
			@BusinessScore = Score, 
			@TangibleEquity = TangibleEquity, 
			@BusinessSeniority = IncorporationDate 
		FROM 
			CustomerAnalyticsCompany 
		WHERE 
			CustomerID = @CustomerId AND
			IsActive = 1
	END
	ELSE
	BEGIN	
		SELECT 
			@BusinessScore = CommercialDelphiScore,
			@BusinessSeniority = IncorporationDate 
		FROM 
			ExperianNonLimitedResults
		WHERE
			RefNumber = @RefNumber AND
			IsActive = 1
	END
	
	IF @BusinessSeniority IS NULL
	BEGIN
		SELECT
			@FirstHmrcDate = MIN(MP_VatReturnRecords.DateFrom)
		FROM
			MP_VatReturnRecords,
			MP_CustomerMarketPlace,
			MP_MarketplaceType
		WHERE
			MP_VatReturnRecords.CustomerMarketPlaceId = MP_CustomerMarketPlace.Id AND
			MP_CustomerMarketPlace.CustomerId = @CustomerId AND
			MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
			MP_MarketplaceType.Name = 'HMRC'
		
		SELECT
			@FirstYodleeDate = MIN(MP_YodleeOrderItemBankTransaction.postDate)
		FROM
			MP_YodleeOrderItemBankTransaction,
			MP_YodleeOrderItem,
			MP_YodleeOrder,
			MP_CustomerMarketPlace,
			MP_MarketplaceType
		WHERE
			MP_CustomerMarketPlace.CustomerId = @CustomerId AND
			MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
			MP_MarketplaceType.Name = 'Yodlee' AND
			MP_YodleeOrder.CustomerMarketPlaceId = MP_CustomerMarketPlace.Id AND
			MP_YodleeOrderItem.OrderId = MP_YodleeOrder.Id AND
			MP_YodleeOrderItemBankTransaction.OrderItemId = MP_YodleeOrderItem.Id
		
		IF @FirstHmrcDate IS NULL AND @FirstYodleeDate IS NOT NULL
			SET @BusinessSeniority = @FirstYodleeDate
		ELSE IF @FirstHmrcDate IS NOT NULL AND @FirstYodleeDate IS NULL
			SET @BusinessSeniority = @FirstHmrcDate
		ELSE IF @FirstHmrcDate IS NULL AND @FirstYodleeDate IS NULL
			SET @BusinessSeniority = CONVERT(DATE, DATEADD(yy, -1, @CalculationTime)) 
		ELSE
		BEGIN -- Both are not null
			IF @FirstHmrcDate > @FirstYodleeDate
				SET @BusinessSeniority = @FirstYodleeDate
			ELSE
				SET @BusinessSeniority = @FirstHmrcDate
		END
	END
	
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
		@NumberOfOnlineStores = COUNT(1) 
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND
		MP_CustomerMarketPlace.MarketPlaceId = MP_MarketplaceType.Id AND
		MP_MarketplaceType.Name IN ('eBay', 'Amazon', 'Pay Pal')
			
	SELECT
		cmp.Id AS CustomerMarketPlaceId,
		MAX(fb.Created) AS MaxCreated
	INTO
		#MaxAmazonCreated
	FROM
		MP_AmazonFeedback fb
		INNER JOIN MP_CustomerMarketPlace cmp ON fb.CustomerMarketPlaceId = cmp.Id
	WHERE
		cmp.CustomerId = @CustomerId
	GROUP BY
		cmp.Id

	SELECT
		@AmazonPositiveFeedbacks = SUM(fbi.Positive)
	FROM
		MP_AmazonFeedback fb
		INNER JOIN #MaxAmazonCreated mc
			ON fb.CustomerMarketPlaceId = mc.CustomerMarketPlaceId
			AND fb.Created = mc.MaxCreated
		INNER JOIN MP_AmazonFeedbackItem fbi
			ON fbi.AmazonFeedbackId = fb.Id
			AND fbi.TimePeriodId = 5 -- 'Lifetime'

	SELECT
		cmp.Id AS CustomerMarketPlaceId,
		MAX(fb.Created) AS MaxCreated
	INTO
		#MaxEbayCreated
	FROM
		MP_EbayFeedback fb
		INNER JOIN MP_CustomerMarketPlace cmp ON fb.CustomerMarketPlaceId = cmp.Id
	WHERE
		cmp.CustomerId = @CustomerId
	GROUP BY
		cmp.Id

	SELECT
		@EbayPositiveFeedbacks = SUM(fbi.Positive)
	FROM
		MP_EbayFeedback fb
		INNER JOIN #MaxEbayCreated mc
			ON fb.CustomerMarketPlaceId = mc.CustomerMarketPlaceId
			AND fb.Created = mc.MaxCreated
		INNER JOIN MP_EbayFeedbackItem fbi
			ON fbi.EbayFeedbackId = fb.Id
			AND fbi.TimePeriodId = 6 -- '0'
	
	DROP TABLE #MaxAmazonCreated
	DROP TABLE #MaxEbayCreated
	
	SELECT
		@NumOfPaypalTransactions = COUNT(1)
	FROM
		MP_CustomerMarketPlace,
		MP_PayPalTransaction,
		MP_PayPalTransactionItem2,
		MP_MarketplaceType
	WHERE
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND
		MP_CustomerMarketPlace.MarketPlaceId = MP_MarketplaceType.Id AND
		MP_MarketplaceType.Name = 'Pay Pal' AND
		MP_PayPalTransaction.CustomerMarketPlaceId = MP_CustomerMarketPlace.Id AND
		MP_PayPalTransactionItem2.TransactionId = MP_PayPalTransaction.Id AND
		MP_PayPalTransactionItem2.GrossAmount > 0
				
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
		@TotalZooplaValue AS TotalZooplaValue,
		@EarliestHmrcLastUpdateDate AS EarliestHmrcLastUpdateDate,
		@EarliestYodleeLastUpdateDate AS EarliestYodleeLastUpdateDate,
		@NumberOfOnlineStores AS NumberOfOnlineStores,
		@AmazonPositiveFeedbacks AS AmazonPositiveFeedbacks,
		@EbayPositiveFeedbacks AS EbayPositiveFeedbacks,
		@NumOfPaypalTransactions AS NumOfPaypalTransactions
END
GO
