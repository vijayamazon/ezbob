IF OBJECT_ID('Temp_GetAllCustomersForMedalComparison') IS NULL
	EXECUTE('CREATE PROCEDURE Temp_GetAllCustomersForMedalComparison AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE Temp_GetAllCustomersForMedalComparison

AS
BEGIN
	DECLARE		
		@TypeOfBusiness NVARCHAR(50),
		@NumOfHmrcMps INT,
		@NumOfYodleeMps INT,
		@NumOfEbayAmazonPayPalMps INT,
		@CompanyScore INT,
		@ConsumerScore INT,
		@CustomerId INT,
		@EarliestHmrcLastUpdateDate DATETIME,
		@EarliestYodleeLastUpdateDate DATETIME
		
	CREATE TABLE #RelevantData
	(
		CustomerId INT,
		TypeOfBusiness NVARCHAR(50),
		NumOfHmrcMps INT,
		NumOfYodleeMps INT,
		NumOfEbayAmazonPayPalMps INT,
		CompanyScore INT,
		ConsumerScore INT,
		EarliestHmrcLastUpdateDate DATETIME,
		EarliestYodleeLastUpdateDate DATETIME
	)
	
	DECLARE cur CURSOR FOR 
	SELECT 
		Id
	FROM 
		Customer
	WHERE
		IsTest = 0 AND
		WizardStep = 4
	ORDER BY
		Id DESC
		
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @TypeOfBusiness = NULL
		SET @CompanyScore = NULL
		SET @ConsumerScore = NULL		
		SET @NumOfYodleeMps = NULL
		SET @EarliestYodleeLastUpdateDate = NULL
		SET @NumOfHmrcMps = NULL
		SET @EarliestHmrcLastUpdateDate = NULL
		SET @NumOfEbayAmazonPayPalMps = NULL
		
		SELECT 
			@TypeOfBusiness = TypeOfBusiness
		FROM 
			Customer 
		WHERE 
			Id = @CustomerId
			
		SELECT
			@CompanyScore = Score
		FROM 
			dbo.udfGetCustomerCompanyAnalytics(@CustomerId, NULL, 0, 0, 0)

		SET @CompanyScore = ISNULL(@CompanyScore, 0)

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
			@NumOfYodleeMps = COUNT(1),
			@EarliestYodleeLastUpdateDate = MIN(UpdatingEnd)
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
			@NumOfHmrcMps = COUNT(1),
			@EarliestHmrcLastUpdateDate = MIN(UpdatingEnd)
		FROM
			MP_CustomerMarketPlace,
			MP_MarketplaceType
		WHERE
			MP_MarketplaceType.Name = 'HMRC' AND
			MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
			MP_CustomerMarketPlace.CustomerId = @CustomerId
	
		INSERT INTO #RelevantData VALUES (@CustomerId, @TypeOfBusiness, @NumOfHmrcMps, @NumOfYodleeMps, @NumOfEbayAmazonPayPalMps, @CompanyScore, @ConsumerScore, @EarliestHmrcLastUpdateDate, @EarliestYodleeLastUpdateDate)
		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur
	
	SELECT CustomerId, TypeOfBusiness, NumOfHmrcMps, NumOfYodleeMps, NumOfEbayAmazonPayPalMps, CompanyScore, ConsumerScore, EarliestHmrcLastUpdateDate, EarliestYodleeLastUpdateDate FROM #RelevantData
	
	DROP TABLE #RelevantData
END
GO
