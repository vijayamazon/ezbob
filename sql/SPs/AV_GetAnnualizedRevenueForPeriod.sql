SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetAnnualizedRevenueForPeriod') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetAnnualizedRevenueForPeriod AS SELECT 1')
GO


ALTER PROCEDURE AV_GetAnnualizedRevenueForPeriod
@CustomerMarketPlaceId INT
AS
BEGIN
	DECLARE @EbayId INT = (SELECT Id FROM MP_MarketplaceType WHERE InternalId= 'A7120CB7-4C93-459B-9901-0E95E7281B59')
	DECLARE @AmazonId INT = (SELECT Id FROM MP_MarketplaceType WHERE InternalId= 'A4920125-411F-4BB9-A52D-27E8A00D0A3B')
	DECLARE @PaypalId INT = (SELECT Id FROM MP_MarketplaceType WHERE InternalId= '3FA5E327-FCFD-483B-BA5A-DC1815747A28')
	
	DECLARE @MPTypeId INT = (SELECT MarketPlaceId FROM MP_CustomerMarketPlace WHERE Id=@CustomerMarketPlaceId)
	
	DECLARE @AnnualizedRevenue1M DECIMAL(18,4) = 0
	DECLARE @AnnualizedRevenue3M DECIMAL(18,4) = 0
	DECLARE @AnnualizedRevenue6M DECIMAL(18,4) = 0
	DECLARE @AnnualizedRevenue1Y DECIMAL(18,4) = 0
	DECLARE @Revenue1Y DECIMAL(18,4) = 0
	
	DECLARE @LastCheckDate DATETIME = (SELECT max(Updated) FROM MP_AnalyisisFunctionValues WHERE CustomerMarketPlaceId=@CustomerMarketPlaceId)
	
	IF @LastCheckDate IS NULL 
	BEGIN
		SELECT @AnnualizedRevenue1M AS AnnualizedRevenue1M, @AnnualizedRevenue3M AS AnnualizedRevenue3M, @AnnualizedRevenue6M AS AnnualizedRevenue6M, @AnnualizedRevenue1Y AS AnnualizedRevenue1Y, @Revenue1Y AS Revenue1Y
		RETURN
	END 
	
	IF (@MPTypeId = @EbayId)
	BEGIN
		SELECT @AnnualizedRevenue1M = isnull(CAST(av.ValueFloat AS DECIMAL(18,4)),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '29C87037-2133-4873-9208-96A4F0163D54' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId = '318795D7-C51D-4B18-8E1F-5A563B3091F4' -- 1m
		
		SELECT @AnnualizedRevenue3M = isnull(CAST(av.ValueFloat AS DECIMAL(18,4)),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '29C87037-2133-4873-9208-96A4F0163D54' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId = 'AA13A708-5230-4F24-895F-E05D513278BD' -- 3m
		
		SELECT @AnnualizedRevenue6M = isnull(CAST(av.ValueFloat AS DECIMAL(18,4)),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '29C87037-2133-4873-9208-96A4F0163D54' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId = '33E0E7AE-92E0-4AAB-A042-10CF34526368' -- 6m
		
		SELECT @AnnualizedRevenue1Y = isnull(CAST(av.ValueFloat AS DECIMAL(18,4)),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '29C87037-2133-4873-9208-96A4F0163D54' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId = '1F9E6CEF-7251-4E1C-AC35-801265E732CD' -- 1y
		
		SELECT @Revenue1Y = isnull(CAST(av.ValueFloat AS DECIMAL(18,4)),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = 'B90CA21F-069B-4076-BB87-D6FC09457971' --TotalSumOfOrders
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId = '1F9E6CEF-7251-4E1C-AC35-801265E732CD' -- 1y
	END
	
	IF (@MPTypeId = @AmazonId)
	BEGIN
	    SELECT @AnnualizedRevenue1M = isnull(min(CAST(av.ValueFloat AS DECIMAL(18,4))),0) 
	    FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '1F5C801E-B845-400C-BA34-8F2552165B74' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='318795D7-C51D-4B18-8E1F-5A563B3091F4' -- 1m
		
		SELECT @AnnualizedRevenue3M = isnull(min(CAST(av.ValueFloat AS DECIMAL(18,4))),0) 
	    FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '1F5C801E-B845-400C-BA34-8F2552165B74' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='AA13A708-5230-4F24-895F-E05D513278BD' -- 3m
		
		SELECT @AnnualizedRevenue6M = isnull(min(CAST(av.ValueFloat AS DECIMAL(18,4))),0) 
	    FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '1F5C801E-B845-400C-BA34-8F2552165B74' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='33E0E7AE-92E0-4AAB-A042-10CF34526368' -- 6m
		
		SELECT @AnnualizedRevenue1Y = isnull(min(CAST(av.ValueFloat AS DECIMAL(18,4))),0) 
	    FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '1F5C801E-B845-400C-BA34-8F2552165B74' --TotalSumOfOrdersAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='1F9E6CEF-7251-4E1C-AC35-801265E732CD' -- 1y
		
		SELECT @Revenue1Y = isnull(min(CAST(av.ValueFloat AS DECIMAL(18,4))),0) 
	    FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '63235650-ACD7-4F73-9537-5D762B0B7D0A' --TotalSumOfOrders
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='1F9E6CEF-7251-4E1C-AC35-801265E732CD' -- 1y
	END
	
	IF (@MPTypeId = @PaypalId)
	BEGIN
		SELECT @AnnualizedRevenue1M = isnull(min(CAST(av.Value AS DECIMAL(18,4))),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '455D0657-4D4F-4494-85F5-F762E67D1B01' --TotalNetInPaymentsAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='318795D7-C51D-4B18-8E1F-5A563B3091F4' -- 1m
		
		SELECT @AnnualizedRevenue3M = isnull(min(CAST(av.Value AS DECIMAL(18,4))),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '455D0657-4D4F-4494-85F5-F762E67D1B01' --TotalNetInPaymentsAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='AA13A708-5230-4F24-895F-E05D513278BD' -- 3m
		
		SELECT @AnnualizedRevenue6M = isnull(min(CAST(av.Value AS DECIMAL(18,4))),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '455D0657-4D4F-4494-85F5-F762E67D1B01' --TotalNetInPaymentsAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='33E0E7AE-92E0-4AAB-A042-10CF34526368' -- 6m
		
		SELECT @AnnualizedRevenue1Y = isnull(min(CAST(av.Value AS DECIMAL(18,4))),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '455D0657-4D4F-4494-85F5-F762E67D1B01' --TotalNetInPaymentsAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='1F9E6CEF-7251-4E1C-AC35-801265E732CD' -- 1y
		
		SELECT @Revenue1Y = isnull(min(CAST(av.Value AS DECIMAL(18,4))),0) 
		FROM MP_AnalyisisFunctionValues av INNER JOIN MP_AnalyisisFunction af ON af.Id = av.AnalyisisFunctionId
		INNER JOIN MP_AnalysisFunctionTimePeriod t ON t.Id = av.AnalysisFunctionTimePeriodId
		WHERE af.InternalId = '455D0657-4D4F-4494-85F5-F762E67D1B01' --TotalNetInPaymentsAnnualized
		AND av.CustomerMarketPlaceId=@CustomerMarketPlaceId
		AND av.Updated=@LastCheckDate
		AND t.InternalId='1F9E6CEF-7251-4E1C-AC35-801265E732CD' -- 1y
	END
	
	
	SELECT @AnnualizedRevenue1M AS AnnualizedRevenue1M, @AnnualizedRevenue3M AS AnnualizedRevenue3M, @AnnualizedRevenue6M AS AnnualizedRevenue6M, @AnnualizedRevenue1Y AS AnnualizedRevenue1Y, @Revenue1Y AS Revenue1Y
END
GO
