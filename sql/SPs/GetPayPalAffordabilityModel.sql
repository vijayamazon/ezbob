IF OBJECT_ID('GetPayPalAffordabilityModel') IS NULL
	EXECUTE('CREATE PROCEDURE GetPayPalAffordabilityModel AS SELECT 1')
GO

ALTER PROCEDURE GetPayPalAffordabilityModel
@CustomerId INT
AS
BEGIN
	DECLARE 
		@PayPalMpId INT,
		@RevenuesAnalysisFunctionId INT,
		@MpId INT,
		@Revenues DECIMAL(18,6)
	
	SELECT @Revenues = 0
	
	SELECT 
		@PayPalMpId = Id 
	FROM 
		MP_MarketplaceType 
	WHERE 
		Name = 'Pay Pal'
	
	SELECT 
		@RevenuesAnalysisFunctionId = Id 
	FROM 
		MP_AnalyisisFunction 
	WHERE 
		MarketPlaceId = @PayPalMpId AND 
		Name = 'TotalNetInPaymentsAnnualized'
	
	DECLARE cur CURSOR FOR 
		SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId AND MarketPlaceId = @PayPalMpId
	OPEN cur
	FETCH NEXT FROM cur INTO @MpId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @LastUpdateDate DATETIME,
			@MpRevenues FLOAT

		SELECT @LastUpdateDate = MAX(Created) FROM MP_PayPalTransaction WHERE CustomerMarketPlaceId = @MpId
				
		SELECT TOP 1 @MpRevenues = ValueFloat FROM MP_AnalyisisFunctionValues WHERE CustomerMarketPlaceId = @MpId AND AnalyisisFunctionId = @RevenuesAnalysisFunctionId AND AnalysisFunctionTimePeriodId < 5 ORDER BY AnalysisFunctionTimePeriodId DESC
		
		IF @MpRevenues IS NOT NULL
		SELECT @Revenues = @Revenues + @MpRevenues
		
		FETCH NEXT FROM cur INTO @MpId
	END
	CLOSE cur
	DEALLOCATE cur	
	
	SELECT 
		MIN(OriginationDate) AS DateFrom, 
		MAX(MP_PayPalTransaction.Created) AS DateTo,
		@Revenues AS Revenues
	FROM 
		MP_CustomerMarketPlace,
		MP_MarketplaceType,
		MP_PayPalTransaction 
	WHERE 
		MP_PayPalTransaction.CustomerMarketPlaceId = MP_CustomerMarketPlace.Id AND
		MP_CustomerMarketPlace.MarketPlaceId = @PayPalMpId AND
		CustomerId = @CustomerId
END
GO
