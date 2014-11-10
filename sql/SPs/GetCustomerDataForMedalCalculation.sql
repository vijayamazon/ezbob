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
		@ServiceLogId BIGINT,
		@EarliestHmrcLastUpdateDate DATETIME,
		@EarliestYodleeLastUpdateDate DATETIME
		
	SELECT 
		@TypeOfBusiness = TypeOfBusiness
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId
		
	IF @TypeOfBusiness = 'LLP' OR @TypeOfBusiness = 'Limited'
	BEGIN
		SELECT 
			@CompanyScore = Score 
		FROM 
			CustomerAnalyticsCompany 
		WHERE 
			CustomerID = @CustomerId AND
			IsActive = 1
	END
	ELSE
	BEGIN
		SELECT TOP 1
			@ServiceLogId = Id
		FROM
			MP_ServiceLog
		WHERE
			ServiceType = 'E-SeriesNonLimitedData' AND
			CustomerId = @CustomerId
		ORDER BY 
			Id DESC
	
		SELECT 
			@CompanyScore = CommercialDelphiScore
		FROM 
			ExperianNonLimitedResults
		WHERE
			ServiceLogId = @ServiceLogId AND
			IsActive = 1
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
	
	SELECT
		@NumOfHmrcMps = COUNT(1)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'HMRC' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId
	
	SELECT
		@NumOfYodleeMps = COUNT(1)
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'Yodlee' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId
		
	SELECT
		@NumOfEbayAmazonPayPalMps = COUNT(1) 
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_CustomerMarketPlace.CustomerId = @CustomerId AND
		MP_CustomerMarketPlace.MarketPlaceId = MP_MarketplaceType.Id AND
		MP_MarketplaceType.Name IN ('eBay', 'Amazon', 'Pay Pal')
		
	SELECT
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
