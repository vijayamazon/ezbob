IF OBJECT_ID('GetCustomerDataForMedalCalculation') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerDataForMedalCalculation AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerDataForMedalCalculation
@CustomerId INT,
@Now DATETIME
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
		@TypeOfBusiness NVARCHAR(50),
		@NumOfHmrcMps INT,
		@NumOfYodleeMps INT,
		@NumOfEbayAmazonPayPalMps INT,
		@CompanyScore INT,
		@ConsumerScore INT,
		@EarliestHmrcLastUpdateDate DATETIME,
		@EarliestYodleeLastUpdateDate DATETIME,
		@LastCashRequestID BIGINT,
		@NLLastCashRequestID BIGINT
		
	------------------------------------------------------------------------------

	SELECT
		@TypeOfBusiness = TypeOfBusiness
	FROM
		Customer
	WHERE
		Id = @CustomerId

	------------------------------------------------------------------------------

	SET @CompanyScore = ISNULL((
		SELECT Score
		FROM dbo.udfGetCustomerCompanyAnalytics(@CustomerId, @Now, 0, 0, 0)
	), 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SET @ConsumerScore = ISNULL((
		SELECT
			MinScore
		FROM	
			dbo.udfGetCustomerScoreAnalytics(@CustomerID, @Now)
	), 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	-- Suppress 'Warning: Null value is eliminated by an aggregate or other SET operation.'
	SET ANSI_WARNINGS OFF;

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@NumOfHmrcMps = COUNT(DISTINCT m.Id),
		@EarliestHmrcLastUpdateDate = MIN(m.UpdatingEnd)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND t.InternalId = @HMRC
		INNER JOIN MP_VatReturnRecords r ON m.Id = r.CustomerMarketPlaceId
		INNER JOIN Business b
			ON r.BusinessId = b.Id
			AND b.BelongsToCustomer = 1
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0
		AND
		m.Created < @Now

	------------------------------------------------------------------------------

	SELECT
		@NumOfYodleeMps = COUNT(DISTINCT m.Id),
		@EarliestYodleeLastUpdateDate = MIN(m.UpdatingEnd)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND t.InternalId = @Yodlee
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0
		AND
		m.Created < @Now

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SET ANSI_WARNINGS ON;

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@NumOfEbayAmazonPayPalMps = COUNT(1) 
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t
			ON m.MarketPlaceId = t.Id
			AND t.InternalId IN (@eBay, @Amazon, @PayPal)
	WHERE
		m.CustomerId = @CustomerId
		AND
		m.Disabled = 0
		AND
		m.Created < @Now

	------------------------------------------------------------------------------

	SELECT TOP 1
		@LastCashRequestID = Id,
		@NLLastCashRequestID = nlcr.CashRequestID
	FROM
		CashRequests c left join NL_CashRequests nlcr on nlcr.OldCashRequestID = c.Id
	WHERE
		IdCustomer = @CustomerId
		AND
		CreationDate < @Now
	ORDER BY
		CreationDate DESC,
		Id DESC		

	------------------------------------------------------------------------------

	SELECT
		@TypeOfBusiness AS TypeOfBusiness,
		@NumOfHmrcMps AS NumOfHmrcMps,
		@NumOfYodleeMps AS NumOfYodleeMps,
		@NumOfEbayAmazonPayPalMps AS NumOfEbayAmazonPayPalMps,
		@CompanyScore AS CompanyScore,
		@ConsumerScore AS ConsumerScore,
		@EarliestHmrcLastUpdateDate AS EarliestHmrcLastUpdateDate,
		@EarliestYodleeLastUpdateDate AS EarliestYodleeLastUpdateDate,
		@LastCashRequestID AS LastCashRequestID,
		@NLLastCashRequestID as NLLastCashRequestID
END
GO
