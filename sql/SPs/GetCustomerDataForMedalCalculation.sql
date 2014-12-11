IF OBJECT_ID('GetCustomerDataForMedalCalculation') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerDataForMedalCalculation AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerDataForMedalCalculation
	@CustomerId INT
AS
BEGIN
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
		
	SELECT 
		@TypeOfBusiness = TypeOfBusiness
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId
		
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
			@CompanyScore = isnull(Score, 0)
		FROM 
			CustomerAnalyticsCompany 
		WHERE 
			CustomerID = @CustomerId AND
			IsActive = 1
	END
	ELSE
	BEGIN	
		SELECT 
			@CompanyScore = isnull(CommercialDelphiScore, 0)
		FROM 
			ExperianNonLimitedResults
		WHERE
			RefNumber = @RefNumber AND
			IsActive = 1
	END
	
	SELECT @ConsumerScore = isnull(MIN(ExperianConsumerScore), 0)
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
		@NumOfHmrcMps = COUNT(1)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'HMRC' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND
		MP_CustomerMarketPlace.Disabled = 0
	
	SELECT
		@NumOfYodleeMps = COUNT(1)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'Yodlee' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND 
		MP_CustomerMarketPlace.Disabled = 0
		
	SELECT
		@NumOfEbayAmazonPayPalMps = COUNT(1) 
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND
		MP_CustomerMarketPlace.MarketPlaceId = MP_MarketplaceType.Id AND
		MP_MarketplaceType.Name IN ('eBay', 'Amazon', 'Pay Pal') AND 
		MP_CustomerMarketPlace.Disabled = 0
		
	SELECT
		@EarliestHmrcLastUpdateDate = MIN(UpdatingEnd)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'HMRC' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND 
		MP_CustomerMarketPlace.Disabled = 0
		
	SELECT
		@EarliestYodleeLastUpdateDate = MIN(UpdatingEnd)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'Yodlee' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND 
		MP_CustomerMarketPlace.Disabled = 0

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
