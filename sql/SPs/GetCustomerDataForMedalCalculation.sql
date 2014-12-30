IF OBJECT_ID('GetCustomerDataForMedalCalculation') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerDataForMedalCalculation AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerDataForMedalCalculation
@CustomerId INT
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
		@RefNumber NVARCHAR(50)
		
	------------------------------------------------------------------------------

	SELECT
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
			@CompanyScore = ISNULL(Score, 0)
		FROM 
			CustomerAnalyticsCompany 
		WHERE 
			CustomerID = @CustomerId
			AND
			IsActive = 1
	END
	ELSE BEGIN	
		SELECT 
			@CompanyScore = ISNULL(CommercialDelphiScore, 0)
		FROM 
			ExperianNonLimitedResults
		WHERE
			RefNumber = @RefNumber
			AND
			IsActive = 1
	END
	
	------------------------------------------------------------------------------

	SELECT
		@ConsumerScore = ISNULL(MIN(ExperianConsumerScore), 0)
	FROM	(
		SELECT
			ExperianConsumerScore
		FROM
			Customer
		WHERE
			Id = @CustomerId
		AND
			ExperianConsumerScore IS NOT NULL
		--
		UNION
		--
		SELECT
			ExperianConsumerScore
		FROM
			Director
		WHERE
			CustomerId = @CustomerId
		AND
			ExperianConsumerScore IS NOT NULL
	) AS X
	
	------------------------------------------------------------------------------

	SELECT
		@NumOfHmrcMps = COUNT(1),
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

	------------------------------------------------------------------------------

	SELECT
		@NumOfYodleeMps = COUNT(1),
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

	------------------------------------------------------------------------------

	SELECT
		@TypeOfBusiness AS TypeOfBusiness,
		@NumOfHmrcMps AS NumOfHmrcMps,
		@NumOfYodleeMps AS NumOfYodleeMps,
		@NumOfEbayAmazonPayPalMps AS NumOfEbayAmazonPayPalMps,
		@CompanyScore AS CompanyScore,
		@ConsumerScore AS ConsumerScore,
		@EarliestHmrcLastUpdateDate AS EarliestHmrcLastUpdateDate,
		@EarliestYodleeLastUpdateDate AS EarliestYodleeLastUpdateDate
END
GO
