SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetMedalInputParams') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetMedalInputParams AS SELECT 1')
GO

ALTER PROCEDURE AV_GetMedalInputParams
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @RegistrationDate DATETIME
	DECLARE @MaritalStatus NVARCHAR(100)
	DECLARE @TypeOfBusiness NVARCHAR(100)

	SELECT
		@RegistrationDate = GreetingMailSentDate,
		@MaritalStatus = MaritalStatus,
		@TypeOfBusiness = TypeOfBusiness 
	FROM
		Customer 
	WHERE
		Id = @CustomerId

	-- BusinessScore IncorporationDate TangibleEquity CurrentBalanceSum taken from CustomerAnalyticsCompany
	DECLARE @BusinessScore INT = 0
	DECLARE @IncorporationDate DATETIME = NULL
	DECLARE @TangibleEquity DECIMAL(18,4) = 0
	DECLARE @CurrentBalanceSum DECIMAL(18,4) = 0
	
	IF @TypeOfBusiness IN ('Limited', 'LLP')
	BEGIN
		SELECT
			@BusinessScore = Score,
			@IncorporationDate = IncorporationDate,
			@TangibleEquity = TangibleEquity,
			@CurrentBalanceSum = CurrentBalanceSum  
		FROM
			CustomerAnalyticsCompany 
		WHERE
			CustomerID = @CustomerId
			AND
			IsActive = 1
	END
	ELSE BEGIN
		SELECT
			@IncorporationDate = nl.IncorporationDate,
			@BusinessScore = nl.CommercialDelphiScore
		FROM
			Customer c
			INNER JOIN Company co ON c.CompanyId = co.Id
			INNER JOIN ExperianNonLimitedResults nl ON co.ExperianRefNum = nl.RefNumber
		WHERE
			c.Id = @CustomerId
			AND
			nl.IsActive=1
	END

	-- IncorporationDate if not exist in experian
	
	IF @IncorporationDate IS NULL
	BEGIN
		SELECT
			@IncorporationDate = MIN(x.MinDate)
		FROM (
			SELECT
				MIN(v.DateFrom) MinDate
			FROM
				MP_VatReturnRecords v
				INNER JOIN MP_CustomerMarketPlace m ON m.Id = v.CustomerMarketPlaceId
				INNER JOIN Business b ON v.BusinessId = b.Id AND b.BelongsToCustomer = 1
			WHERE
				m.CustomerId = @CustomerId
				AND
				m.Disabled = 0
				AND
				v.DateFrom IS NOT NULL
			UNION
			SELECT
				MIN(tr.transactionDate) MinDate
			FROM
				MP_CustomerMarketPlace m
				INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = m.Id
				INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
				INNER JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
			WHERE
				m.CustomerId = @CustomerId
				AND
				m.Disabled = 0
				AND
				tr.transactionDate IS NOT NULL
			UNION
			SELECT
				MIN(tr.postDate) MinDate
			FROM
				MP_CustomerMarketPlace m
				INNER JOIN MP_YodleeOrder o ON o.CustomerMarketPlaceId = m.Id
				INNER JOIN MP_YodleeOrderItem i ON i.OrderId = o.Id
				INNER JOIN MP_YodleeOrderItemBankTransaction tr ON tr.OrderItemId = i.Id
			WHERE
				m.CustomerId = @CustomerId
				AND
				m.Disabled = 0
				AND
				tr.postDate IS NOT NULL
		) x
		WHERE x.MinDate IS NOT NULL
	END 

	--NumOfOnTimeLoans
	DECLARE @NumOfOnTimeLoans INT 
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
		@NumOfOnTimeLoans = ISNULL(COUNT(DISTINCT l.Id), 0)
	FROM
		Loan l
		LEFT JOIN late_loans ll ON l.Id = ll.LoanID
	WHERE
		ll.LoanID IS NULL
		AND
		l.CustomerId = @CustomerID
		AND
		l.DateClosed IS NOT NULL

	--NumOfLatePayments
	DECLARE @LateFeeConfigId INT = (
		SELECT Id
		FROM ConfigurationVariables
		WHERE Name = 'LatePaymentCharge'
	)
	
	DECLARE @NumOfLatePayments INT 
	
	SELECT
		@NumOfLatePayments = COUNT(lc.Id) 
	FROM
		Loan l
		INNER JOIN LoanCharges lc ON l.Id = lc.LoanId 
	WHERE
		l.CustomerId = @CustomerId
		AND
		lc.ConfigurationVariableId = @LateFeeConfigId

	--NumOfEarlyPayments
	DECLARE @NumOfEarlyPayments INT 

	SELECT
		@NumOfEarlyPayments = COUNT(ls.Id)
	FROM
		Loan l
		INNER JOIN LoanSchedule ls ON l.Id = ls.LoanId
	WHERE
		l.CustomerId = @CustomerId
		AND
		ls.Status = 'PaidEarly'

	--HMRC
	DECLARE @NumOfHmrc INT 
	DECLARE @HasHmrc BIT = 0

	SELECT
		@NumOfHmrc = COUNT(mp.Id)
	FROM
		MP_CustomerMarketPlace mp
		INNER JOIN MP_MarketplaceType t ON t.Id = mp.MarketPlaceId 
		INNER JOIN MP_VatReturnRecords r ON mp.Id = r.CustomerMarketPlaceId
		INNER JOIN Business b ON r.BusinessId = b.Id AND b.BelongsToCustomer = 1
	WHERE
		t.InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
		AND
		mp.CustomerId = @CustomerId
		AND
		mp.Disabled = 0

	DECLARE @FCFFactor DECIMAL(18,4)
	
	IF @NumOfHmrc > 0 SET @HasHmrc = 1

	SELECT
		@FCFFactor = CAST(Value AS DECIMAL(18, 4))
	FROM
		ConfigurationVariables
	WHERE
		Name = 'FCFFactor'
	
	--NetWorth
	
	--Zoopla
	DECLARE @ZooplaValue INT

	SELECT
		@ZooplaValue = ISNULL(SUM(
			CASE WHEN z.ZooplaEstimateValue = 0 THEN z.AverageSoldPrice1Year ELSE z.ZooplaEstimateValue END
		), 0)  
	FROM
		CustomerAddress ca
		INNER JOIN Zoopla z ON z.CustomerAddressId = ca.addressId 
	WHERE
		CustomerId = @CustomerId
		AND
		IsOwnerAccordingToLandRegistry = 1
	
	------------------------------------------------------------------------------

	DECLARE @ServiceLogId BIGINT
	EXEC GetExperianConsumerServiceLog @CustomerId, @ServiceLogId OUTPUT

	------------------------------------------------------------------------------

	--Mortgages
	DECLARE @Mortages INT

	SELECT
		@Mortages = ISNULL(SUM(ISNULL(Balance, 0)), 0)
	FROM
		ExperianConsumerDataCais c
		INNER JOIN ExperianConsumerData d ON d.Id = c.ExperianConsumerDataId 
	WHERE
		d.ServiceLogId = @ServiceLogId 
		AND
		AccountType IN ('03','16','25','30','31','32','33','34','35','69') 
		AND
		MatchTo = 1 
		AND
		AccountStatus <> 'S'

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	-- Minimal Consumer/Directors score

	DECLARE @ExperianConsumerDataID BIGINT

	SELECT
		@ExperianConsumerDataID = e.Id
	FROM
		ExperianConsumerData e
	WHERE
		e.ServiceLogId = @ServiceLogId

	------------------------------------------------------------------------------

	DECLARE @ConsumerScore INT

	SELECT
		@ConsumerScore = MIN(x.ExperianConsumerScore)
	FROM	(
		SELECT ISNULL(d.BureauScore, 0) AS ExperianConsumerScore
		FROM ExperianConsumerData d
		INNER JOIN MP_ServiceLog l ON d.ServiceLogId = l.Id
		WHERE d.Id = @ExperianConsumerDataID

		UNION

		SELECT ISNULL(d.MinScore, 0) AS ExperianConsumerScore
		FROM CustomerAnalyticsDirector d
		WHERE d.CustomerID = @CustomerId
		AND d.IsActive = 1
	) x

	SET @ConsumerScore = ISNULL(@ConsumerScore, 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	-- First Repayment Date

	DECLARE @FirstRepaymentDate DATETIME = (
		SELECT TOP 1 ls.[Date] 
		FROM Loan l INNER JOIN LoanSchedule ls ON ls.LoanId = l.Id 
		WHERE l.CustomerId = @CustomerId
		ORDER BY ls.[Date]
	)

	--Num of banks (yodlee)

	DECLARE @NumOfBanks INT 
	
	SELECT
		@NumOfBanks = ISNULL(COUNT(mp.Id), 0)
	FROM
		MP_CustomerMarketPlace mp
		INNER JOIN MP_MarketplaceType t ON t.Id = mp.MarketPlaceId
	WHERE
		mp.CustomerId = @CustomerId
		AND
		mp.Disabled = 0
		AND
		t.InternalId = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'

	--Num of ebay amazon and paypal stores

	DECLARE @NumOfStores INT 
	
	SELECT
		@NumOfStores = ISNULL(COUNT(mp.Id), 0)
	FROM
		MP_CustomerMarketPlace mp
		INNER JOIN MP_MarketplaceType t ON t.Id = mp.MarketPlaceId
	WHERE
		mp.CustomerId = @CustomerId
		AND
		mp.Disabled = 0
		AND
		t.InternalId IN (
			'A7120CB7-4C93-459B-9901-0E95E7281B59',
			'A4920125-411F-4BB9-A52D-27E8A00D0A3B',
			'3FA5E327-FCFD-483B-BA5A-DC1815747A28'
		)

	--Config for online cap

	DECLARE @OnlineMedalTurnoverCutoff DECIMAL(18,4) = (
		SELECT CAST(Value AS DECIMAL(18,4))
		FROM ConfigurationVariables
		WHERE Name = 'OnlineMedalTurnoverCutoff'
	)

	-- Return All Params
	SELECT 
		@BusinessScore AS BusinessScore,
		@IncorporationDate AS IncorporationDate,
		@TangibleEquity AS TangibleEquity,
		@ConsumerScore AS ConsumerScore,
		@RegistrationDate AS RegistrationDate,
		@MaritalStatus AS MaritalStatus,
		@TypeOfBusiness AS TypeOfBusiness,
		@NumOfOnTimeLoans AS NumOfOnTimeLoans,
		@NumOfLatePayments AS NumOfLatePayments,
		@NumOfEarlyPayments AS NumOfEarlyPayments,
		@HasHmrc AS HasHmrc,
		@FCFFactor AS FCFFactor,
		@CurrentBalanceSum AS CurrentBalanceSum,
		@ZooplaValue AS ZooplaValue,
		@Mortages AS Mortages,
		@FirstRepaymentDate AS FirstRepaymentDate,
		ISNULL(@NumOfBanks, 0) AS NumOfBanks,
		@NumOfStores AS NumOfStores,
		@OnlineMedalTurnoverCutoff AS OnlineMedalTurnoverCutoff
END
GO
