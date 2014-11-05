SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetMinAnnualizedRevenue') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetMinAnnualizedRevenue AS SELECT 1')
GO

ALTER PROCEDURE AV_GetMinAnnualizedRevenue
@CustomerMarketPlaceId INT
AS
BEGIN

	DECLARE @EbayId INT = (SELECT Id FROM MP_MarketplaceType WHERE InternalId= 'A7120CB7-4C93-459B-9901-0E95E7281B59')
	DECLARE @AmazonId INT = (SELECT Id FROM MP_MarketplaceType WHERE InternalId= 'A4920125-411F-4BB9-A52D-27E8A00D0A3B')
	DECLARE @PaypalId INT = (SELECT Id FROM MP_MarketplaceType WHERE InternalId= '3FA5E327-FCFD-483B-BA5A-DC1815747A28')
	
	DECLARE @MPTypeId INT = (SELECT MarketPlaceId FROM MP_CustomerMarketPlace WHERE Id=@CustomerMarketPlaceId)
	DECLARE @MinAnnualizedRevenue DECIMAL(18,4) = 0
	
	DECLARE @LastCheckDate DATETIME = (SELECT max(Updated) FROM MP_AnalyisisFunctionValues WHERE CustomerMarketPlaceId=@CustomerMarketPlaceId)
	
	IF @LastCheckDate IS NULL 
	BEGIN
		SELECT 0 AS MinAnnualizedRevenue
		RETURN
	END 
	
	IF (@MPTypeId = @EbayId)
	BEGIN
		SELECT @MinAnnualizedRevenue = isnull(min(CAST(av.Value AS DECIMAL(18,4))),0) FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		WHERE af.InternalId = '29C87037-2133-4873-9208-96A4F0163D54' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND av.Value IS NOT NULL
		AND CAST(av.Value AS DECIMAL(18,4))  <> 0
	END
	
	IF (@MPTypeId = @AmazonId)
	BEGIN
	    SELECT @MinAnnualizedRevenue = isnull(min(CAST(av.Value AS DECIMAL(18,4))),0) FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		WHERE af.InternalId = '1F5C801E-B845-400C-BA34-8F2552165B74' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND av.Value IS NOT NULL
		AND CAST(av.Value AS DECIMAL(18,4))  <> 0
	END
	
	IF (@MPTypeId = @PaypalId)
	BEGIN
		SELECT @MinAnnualizedRevenue = isnull(min(CAST(av.Value AS DECIMAL(18,4))),0) FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		WHERE af.InternalId = '455D0657-4D4F-4494-85F5-F762E67D1B01' --TotalNetInPaymentsAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND av.Value IS NOT NULL
		AND CAST(av.Value AS DECIMAL(18,4))  <> 0
	END
	
	
	SELECT @MinAnnualizedRevenue AS MinAnnualizedRevenue
END 
GO