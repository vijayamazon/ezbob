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
		SELECT TOP 1
			@CompanyScore = cac.Score
		FROM 
			CustomerAnalyticsCompany cac
		WHERE 
			cac.CustomerID = @CustomerId
			AND
			cac.AnalyticsDate < @Now
		ORDER BY
			cac.AnalyticsDate DESC
	END
	ELSE BEGIN
		SELECT TOP 1
			@CompanyScore = r.CommercialDelphiScore
		FROM 
			ExperianNonLimitedResults r
			INNER JOIN MP_ServiceLog l ON r.ServiceLogId = l.Id
		WHERE
			r.RefNumber = @RefNumber
			AND
			l.InsertDate < @Now
		ORDER BY
			l.InsertDate DESC
	END

	SET @CompanyScore = ISNULL(@CompanyScore, 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @ServiceLogId BIGINT

	EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT, @Now

	------------------------------------------------------------------------------

	DECLARE @ExperianConsumerDataID BIGINT

	SELECT
		@ExperianConsumerDataID = e.Id
	FROM
		ExperianConsumerData e
	WHERE
		e.ServiceLogId = @ServiceLogId

	------------------------------------------------------------------------------

	SELECT
		@ConsumerScore = MIN(x.ExperianConsumerScore)
	FROM	(
		SELECT ISNULL(d.BureauScore, 0) AS ExperianConsumerScore
		FROM ExperianConsumerData d
		INNER JOIN MP_ServiceLog l ON d.ServiceLogId = l.Id
		WHERE d.Id = @ExperianConsumerDataID
		AND l.InsertDate < @Now

		UNION

		SELECT ISNULL(d.MinScore, 0) AS ExperianConsumerScore
		FROM CustomerAnalyticsDirector d
		WHERE d.CustomerID = @CustomerID
		AND d.AnalyticsDate < @Now
	) x

	SET @ConsumerScore = ISNULL(@ConsumerScore, 0)

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
