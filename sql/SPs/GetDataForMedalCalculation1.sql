IF OBJECT_ID('GetDataForMedalCalculation1') IS NULL
	EXECUTE('CREATE PROCEDURE GetDataForMedalCalculation1 AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetDataForMedalCalculation1
@CustomerId INT,
@CalculationTime DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @eBay   UNIQUEIDENTIFIER = 'A7120CB7-4C93-459B-9901-0E95E7281B59'
	DECLARE @Amazon UNIQUEIDENTIFIER = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'
	DECLARE @PayPal UNIQUEIDENTIFIER = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'
	DECLARE @Yodlee UNIQUEIDENTIFIER = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'
	DECLARE @HMRC   UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	------------------------------------------------------------------------------

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
		@FirstYodleeDate DATETIME,
		@FirstYodleePostDate DATETIME,
		@FirstYodleeTransactionDate DATETIME

	------------------------------------------------------------------------------

	SET @Threshold = 2

	------------------------------------------------------------------------------

	SELECT 
		@EzbobSeniority = GreetingMailSentDate, 
		@MaritalStatus = MaritalStatus,
		@TypeOfBusiness = TypeOfBusiness
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId

	------------------------------------------------------------------------------

	SELECT
		@RefNumber = ExperianRefNum
	FROM
		Customer
		INNER JOIN Company ON Customer.CompanyId = Company.Id
	WHERE
		Customer.Id = @CustomerId

	------------------------------------------------------------------------------

	IF @TypeOfBusiness = 'LLP' OR @TypeOfBusiness = 'Limited'
	BEGIN
		SELECT 
			@BusinessScore = Score, 
			@TangibleEquity = TangibleEquity, 
			@BusinessSeniority = IncorporationDate 
		FROM 
			CustomerAnalyticsCompany 
		WHERE 
			CustomerID = @CustomerId
			AND
			IsActive = 1
	END
	ELSE BEGIN	
		SELECT 
			@BusinessScore = CommercialDelphiScore,
			@BusinessSeniority = IncorporationDate 
		FROM 
			ExperianNonLimitedResults
		WHERE
			RefNumber = @RefNumber
			AND
			IsActive = 1
	END

	------------------------------------------------------------------------------

	IF @BusinessSeniority IS NULL
	BEGIN
		SELECT
			@FirstHmrcDate = MIN(r.DateFrom)
		FROM
			MP_VatReturnRecords r
			INNER JOIN MP_CustomerMarketPlace m
				ON r.CustomerMarketPlaceId = m.Id
				AND m.Disabled = 0
			INNER JOIN MP_MarketplaceType t
				ON t.Id = m.MarketPlaceId
				AND t.InternalId = @HMRC
			INNER JOIN Business b
				ON r.BusinessId = b.Id
				AND b.BelongsToCustomer = 1
		WHERE
			m.CustomerId = @CustomerId

		-----------------------------------------------------------------------------

		SELECT
			@FirstYodleePostDate = MIN(t.postDate),
			@FirstYodleeTransactionDate = MIN(t.transactionDate)
		FROM
			MP_YodleeOrderItemBankTransaction t
			INNER JOIN MP_YodleeOrderItem i
				ON t.OrderItemId = i.Id
			INNER JOIN MP_YodleeOrder o
				ON i.OrderId = o.Id
			INNER JOIN MP_CustomerMarketPlace m
				ON o.CustomerMarketPlaceId = m.Id
				AND m.Disabled = 0
			INNER JOIN MP_MarketplaceType mt
				ON mt.Id = m.MarketPlaceId
				AND mt.InternalId = @Yodlee
		WHERE
			m.CustomerId = @CustomerId

		-----------------------------------------------------------------------------

		SET @FirstYodleeDate = dbo.udfMinDate(
			@FirstYodleePostDate,
			@FirstYodleeTransactionDate
		)
	
		-----------------------------------------------------------------------------

		SET @BusinessSeniority = dbo.udfMinDate(@FirstHmrcDate, @FirstYodleeDate)

		IF @BusinessSeniority IS NULL
			SET @BusinessSeniority = CONVERT(DATE, DATEADD(yy, -1, @CalculationTime)) 
	END

	------------------------------------------------------------------------------

	SELECT
		@ConsumerScore = MIN(ExperianConsumerScore)
	FROM	(
		SELECT ExperianConsumerScore
		FROM Customer
		WHERE Id = @CustomerId
		AND ExperianConsumerScore IS NOT NULL
		--
		UNION
		--
		SELECT ExperianConsumerScore
		FROM Director
		WHERE CustomerId = @CustomerId
		AND ExperianConsumerScore IS NOT NULL
	) AS X

	------------------------------------------------------------------------------

	SET @FirstRepaymentDatePassed = 0

	------------------------------------------------------------------------------

	SELECT 
		@FirstRepaymentDate = MIN(s.Date) 
	FROM 
		LoanSchedule s
		INNER JOIN Loan l ON l.Id = s.LoanId
	WHERE 
		l.CustomerId = @CustomerId

	------------------------------------------------------------------------------

	IF @FirstRepaymentDate IS NOT NULL AND @FirstRepaymentDate < @CalculationTime
		SET @FirstRepaymentDatePassed = 1

	------------------------------------------------------------------------------

	;WITH late_loans AS (
		SELECT DISTINCT
			lst.LoanID
		FROM
			LoanScheduleTransaction lst
			INNER JOIN Loan l ON lst.LoanId = l.Id AND l.CustomerId = @CustomerID
			INNER JOIN LoanSchedule s ON s.Id = lst.ScheduleID
			INNER JOIN LoanTransaction t ON t.Id = lst.TransactionID
		WHERE
			DATEDIFF(day, s.[Date], t.PostDate) > 7
			AND
			(ABS(lst.PrincipalDelta) + ABS(lst.FeesDelta) + ABS(lst.InterestDelta)) > 2
	)
	SELECT
		@OnTimeLoans = ISNULL(COUNT(DISTINCT l.Id), 0)
	FROM
		Loan l
		LEFT JOIN late_loans ll ON l.Id = ll.LoanID
	WHERE
		ll.LoanID IS NULL
		AND
		l.CustomerId = @CustomerID
		AND
		l.DateClosed IS NOT NULL

	------------------------------------------------------------------------------

	SELECT 
		@NumOfLatePayments = COUNT(lc.Id) 
	FROM 
		LoanCharges lc
		INNER JOIN ConfigurationVariables v
			ON v.Id = lc.ConfigurationVariableId
			AND v.Name = 'LatePaymentCharge'
		INNER JOIN Loan l
			ON l.Id = lc.LoanId
	WHERE 
		l.CustomerId = @CustomerId

	------------------------------------------------------------------------------

	SELECT 
		@NumOfEarlyPayments = COUNT(s.id) 
	FROM 
		LoanSchedule s
		INNER JOIN Loan l ON l.Id = s.LoanId
	WHERE 
		s.Status = 'PaidEarly'
		AND
		l.CustomerId = @CustomerId

	------------------------------------------------------------------------------

	SELECT
		@NumOfHmrcMps = COUNT(1),
		@EarliestHmrcLastUpdateDate = MIN(m.UpdatingEnd)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t
			ON t.Id = m.MarketPlaceId
			AND t.InternalId = @HMRC
		INNER JOIN MP_VatReturnRecords r
			ON m.Id = r.CustomerMarketPlaceId
		INNER JOIN Business b
			ON r.BusinessId = b.Id
			AND b.BelongsToCustomer = 1
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0

	------------------------------------------------------------------------------

	SELECT
		@EarliestYodleeLastUpdateDate = MIN(m.UpdatingEnd)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t
			ON t.Id = m.MarketPlaceId
			AND t.InternalId = @Yodlee
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0

	------------------------------------------------------------------------------

	SELECT 
		@TotalZooplaValue = SUM(CASE
			WHEN ZooplaEstimateValue IS NOT NULL AND ZooplaEstimateValue != 0
				THEN ZooplaEstimateValue
			ELSE ISNULL(AverageSoldPrice1Year, 0)
		END)
	FROM 
		Zoopla z
		INNER JOIN CustomerAddress a
			ON a.addressId = z.CustomerAddressId
			AND a.IsOwnerAccordingToLandRegistry = 1
	WHERE 
		CustomerId = @CustomerId

	------------------------------------------------------------------------------

	SELECT
		@NumberOfOnlineStores = COUNT(1) 
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND t.InternalId IN (@eBay, @Amazon, @PayPal)
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0
			
	------------------------------------------------------------------------------

	SELECT
		cmp.Id AS CustomerMarketPlaceId,
		MAX(fb.Created) AS MaxCreated
	INTO
		#MaxAmazonCreated
	FROM
		MP_AmazonFeedback fb
		INNER JOIN MP_CustomerMarketPlace cmp ON fb.CustomerMarketPlaceId = cmp.Id
	WHERE
		cmp.CustomerId = @CustomerId AND 
		cmp.Disabled = 0
	GROUP BY
		cmp.Id

	------------------------------------------------------------------------------

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

	------------------------------------------------------------------------------

	SELECT
		cmp.Id AS CustomerMarketPlaceId,
		MAX(fb.Created) AS MaxCreated
	INTO
		#MaxEbayCreated
	FROM
		MP_EbayFeedback fb
		INNER JOIN MP_CustomerMarketPlace cmp ON fb.CustomerMarketPlaceId = cmp.Id
	WHERE
		cmp.CustomerId = @CustomerId AND
		cmp.Disabled = 0
	GROUP BY
		cmp.Id

	------------------------------------------------------------------------------

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
	
	------------------------------------------------------------------------------

	DROP TABLE #MaxAmazonCreated
	DROP TABLE #MaxEbayCreated
	
	------------------------------------------------------------------------------

	SELECT
		@NumOfPaypalTransactions = COUNT(1)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_PayPalTransaction t
			ON t.CustomerMarketPlaceId = m.Id
		INNER JOIN MP_PayPalTransactionItem2 ti
			ON ti.TransactionId = t.Id
			AND ti.GrossAmount > 0
		INNER JOIN MP_MarketplaceType mt
			ON m.MarketPlaceId = mt.Id
			AND mt.InternalId = @PayPal
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0

	------------------------------------------------------------------------------

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
		@NumOfHmrcMps AS NumOfHmrcMps,
		@TotalZooplaValue AS TotalZooplaValue,
		@EarliestHmrcLastUpdateDate AS EarliestHmrcLastUpdateDate,
		@EarliestYodleeLastUpdateDate AS EarliestYodleeLastUpdateDate,
		@NumberOfOnlineStores AS NumberOfOnlineStores,
		@AmazonPositiveFeedbacks AS AmazonPositiveFeedbacks,
		@EbayPositiveFeedbacks AS EbayPositiveFeedbacks,
		@NumOfPaypalTransactions AS NumOfPaypalTransactions

	------------------------------------------------------------------------------
END
GO
