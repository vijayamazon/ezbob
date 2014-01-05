IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetPayPalAggregations') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetPayPalAggregations
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetPayPalAggregations 
	(@CustomerId INT)
AS
BEGIN
	DECLARE 
		@TotalSumOfOrders3M INT,
		@TotalSumOfOrders1Y INT,
		@TotalSumOfOrders1M INT,
		@TotalSumOfOrders6M INT,
		@PayPalCount INT,
		@UserId INT


	DECLARE @InternalId_1M uniqueidentifier = '318795D7-C51D-4B18-8E1F-5A563B3091F4'-- 30 days - 1 months
	DECLARE @InternalId_3M uniqueidentifier = 'AA13A708-5230-4F24-895F-E05D513278BD'-- 90 days - 3 months
	DECLARE @InternalId_6M uniqueidentifier = '33E0E7AE-92E0-4AAB-A042-10CF34526368'-- 180 days - 6 months
	DECLARE @InternalId_1Y uniqueidentifier = '1F9E6CEF-7251-4E1C-AC35-801265E732CD'-- 1 year - 12 months - 365 days

	SET @UserId = @CustomerId


	SELECT 
		@TotalSumOfOrders1M = ISNULL(Sum(v.ValueInt),0) + ISNULL(Sum(v.ValueFloat),0)
	FROM 
		MP_AnalyisisFunctionValues v
	WHERE 
		v.CustomerMarketPlaceUpdatingHistoryRecordId IN
			(
				SELECT 
					maxUpdateTable.UpdateHistoryId
				FROM 
					GetId_MaxUpdated(@UserId) maxUpdateTable
			) AND 
		v.AnalysisFunctionTimePeriodId IN
			(
				SELECT 
					id
				FROM 
					MP_AnalysisFunctionTimePeriod
				WHERE InternalId = @InternalId_1M 
			) AND 
		v.AnalyisisFunctionId IN
			(
				SELECT 
					id
				FROM 
					MP_AnalyisisFunction
				WHERE 
					InternalId = '9370A525-890D-402B-9BAA-5C89E9905CA2'
			)

	SELECT 
		@TotalSumOfOrders3M = ISNULL(Sum(v.ValueInt),0) + ISNULL(Sum(v.ValueFloat),0)
	FROM 
		MP_AnalyisisFunctionValues v
	WHERE 
		v.CustomerMarketPlaceUpdatingHistoryRecordId IN
			(
				SELECT 
					maxUpdateTable.UpdateHistoryId
				FROM 
					GetId_MaxUpdated(@UserId) maxUpdateTable
			) AND 
		v.AnalysisFunctionTimePeriodId IN
			(
				SELECT
					Max(id)
				FROM 
					MP_AnalysisFunctionTimePeriod
				WHERE InternalId = @InternalId_3M 
			) AND 
		v.AnalyisisFunctionId IN
			(
				SELECT id
				FROM MP_AnalyisisFunction
				WHERE InternalId = '9370A525-890D-402B-9BAA-5C89E9905CA2'
				 
			)

	SELECT 
		@TotalSumOfOrders6M=ISNULL(Sum(v.ValueInt),0) + ISNULL(Sum(v.ValueFloat),0)
	FROM 
		MP_AnalyisisFunctionValues v
	WHERE 
		v.CustomerMarketPlaceUpdatingHistoryRecordId IN 
			(
				SELECT 
					maxUpdateTable.UpdateHistoryId
				FROM 
					GetId_MaxUpdated(@UserId) maxUpdateTable
			) AND 
		v.AnalysisFunctionTimePeriodId IN
			(
				SELECT 
					id
				FROM 
					MP_AnalysisFunctionTimePeriod
				WHERE 
					InternalId = @InternalId_6M
			) AND 
		v.AnalyisisFunctionId IN
			(
				SELECT 
					id
				FROM 
					MP_AnalyisisFunction
				WHERE 
					InternalId = '9370A525-890D-402B-9BAA-5C89E9905CA2'
			)

	SELECT 
		@TotalSumOfOrders1Y = ISNULL(Sum(v.ValueInt),0) + ISNULL(Sum(v.ValueFloat),0)
	FROM 
		MP_AnalyisisFunctionValues v
	WHERE 
		v.CustomerMarketPlaceUpdatingHistoryRecordId IN 
			(
				SELECT 
					maxUpdateTable.UpdateHistoryId
				FROM 
					GetId_MaxUpdated(@UserId) maxUpdateTable
			) AND 
		v.AnalysisFunctionTimePeriodId IN
			(
				SELECT 
					id
				FROM 
					MP_AnalysisFunctionTimePeriod
				WHERE 
					InternalId = @InternalId_1Y
			) AND 
		v.AnalyisisFunctionId IN
			(
				SELECT 
					id
				FROM 
					MP_AnalyisisFunction
				WHERE 
					InternalId = '9370A525-890D-402B-9BAA-5C89E9905CA2'
			)

	SELECT 
		@PayPalCount = COUNT(cmp.id) 
	FROM 
		MP_CustomerMarketPlace cmp
		LEFT JOIN MP_MarketplaceType mpt ON mpt.Id = cmp.MarketPlaceId
	WHERE 
		cmp.CustomerId = @UserId AND 
		mpt.InternalId = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'

	IF @TotalSumOfOrders1Y = 0 AND @TotalSumOfOrders6M = 0 AND @TotalSumOfOrders3M = 0 AND @TotalSumOfOrders1M > 0 
		SET @TotalSumOfOrders1Y = @TotalSumOfOrders1M

	IF @TotalSumOfOrders1Y = 0 AND @TotalSumOfOrders6M = 0 AND @TotalSumOfOrders3M > 0 
		SET @TotalSumOfOrders1Y = @TotalSumOfOrders3M

	IF @TotalSumOfOrders1Y = 0 AND @TotalSumOfOrders6M > 0  
		SET @TotalSumOfOrders1Y = @TotalSumOfOrders6M

	IF @TotalSumOfOrders3M = 0 AND @TotalSumOfOrders1M > 0  
		SET @TotalSumOfOrders3M = @TotalSumOfOrders1M


	SELECT 
		@TotalSumOfOrders3M AS PayPal_TotalSumOfOrders3M,  
		@TotalSumOfOrders1Y AS PayPal_TotalSumOfOrders1Y, 
		@PayPalCount AS PayPal_NumberOfStores
END
GO
