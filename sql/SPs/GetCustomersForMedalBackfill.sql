IF OBJECT_ID('GetCustomersForMedalBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomersForMedalBackfill AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomersForMedalBackfill
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE 
		@CustomerId INT,
		@NumOfEntriesInOfflineScoringTable INT,
		@NumOfHmrcMps INT,
		@CalculationTime DATETIME
	
	SELECT
		Id AS CustomerId,
		TypeOfBusiness,
		0 AS NumOfHmrcMps,
		0 AS NumOfEntriesInOfflineScoringTable,
		CAST (NULL AS DATETIME) AS CalculationTime
	INTO
		#GetCustomersForMedalBackfillTemp
	FROM
		Customer,
		WizardStepTypes
	WHERE
		WizardStep = WizardStepTypeId AND
		TheLastOne = 1

	DECLARE cur CURSOR FOR 
	SELECT 
		CustomerId
	FROM 
		#GetCustomersForMedalBackfillTemp
		
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
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
			@NumOfEntriesInOfflineScoringTable = COUNT(1) 
		FROM 
			OfflineScoring 
		WHERE 
			CustomerId = @CustomerId
			
		SELECT 
			@CalculationTime = MAX(UpdatingEnd)
		FROM 
			MP_CustomerMarketPlace
		WHERE
			MP_CustomerMarketPlace.CustomerId = @CustomerId
		
		UPDATE 
			#GetCustomersForMedalBackfillTemp
		SET 
			NumOfHmrcMps = @NumOfHmrcMps,
			NumOfEntriesInOfflineScoringTable = @NumOfEntriesInOfflineScoringTable,
			CalculationTime = @CalculationTime
		WHERE 
			CustomerId = @CustomerId
		
		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur
	
	SELECT CustomerId, TypeOfBusiness, NumOfHmrcMps, NumOfEntriesInOfflineScoringTable, CalculationTime FROM #GetCustomersForMedalBackfillTemp
	DROP TABLE #GetCustomersForMedalBackfillTemp
END
GO
