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
		@ServiceLogId BIGINT
		
	CREATE TABLE #RelevantData
	(
		CustomerId INT,
		TypeOfBusiness NVARCHAR(50),
		NumOfHmrcMps INT,
		NumOfYodleeMps INT,
		NumOfEbayAmazonPayPalMps INT,
		CompanyScore INT,
		ConsumerScore INT
	)
	
	DECLARE cur CURSOR FOR 
	SELECT 
		Id
	FROM 
		Customer
	WHERE
		IsTest = 0 AND
		WizardStep = 4 AND
		GreetingMailSentDate >= '1 Sep 2014'
		
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
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
	
		INSERT INTO #RelevantData VALUES (@CustomerId, @TypeOfBusiness, @NumOfHmrcMps, @NumOfYodleeMps, @NumOfEbayAmazonPayPalMps, @CompanyScore, @ConsumerScore)
		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur
	
	SELECT CustomerId, TypeOfBusiness, NumOfHmrcMps, NumOfYodleeMps, NumOfEbayAmazonPayPalMps, CompanyScore, ConsumerScore FROM #RelevantData
	
	DROP TABLE #RelevantData
END
GO
