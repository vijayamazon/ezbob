IF OBJECT_ID('UpdateMpTotalsEbay') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsEbay AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsEbay
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @MpID INT

	EXECUTE GetMarketplaceFromHistoryID 'eBay', @HistoryID, @MpID OUTPUT

	IF @MpID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	-- 1. Select all the full months.

	SELECT
		i.Id,
		i.RangeMarker,
		i.StartDate,
		i.Transactions,
		i.Listings,
		i.Successful,
		i.ItemsSold,
		i.Revenue
	INTO
		#order_items
	FROM
		MP_TeraPeakOrderItem i
		INNER JOIN MP_TeraPeakOrder o ON i.TeraPeakOrderId = o.Id
	WHERE
		i.RangeMarker = 0
		AND
		o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID

	-- 2. Append the last partial month which is greater than any existing full month.

	DECLARE @MaxFull DATETIME

	SELECT
		@MaxFull = MAX(StartDate)
	FROM
		#order_items

	INSERT INTO #order_items(Id, RangeMarker, StartDate, Transactions, Listings, Successful, ItemsSold, Revenue)
	SELECT TOP 1
		i.Id,
		i.RangeMarker,
		i.StartDate,
		i.Transactions,
		i.Listings,
		i.Successful,
		i.ItemsSold,
		i.Revenue
	FROM
		MP_TeraPeakOrderItem i
		INNER JOIN MP_TeraPeakOrder o ON i.TeraPeakOrderId = o.Id
	WHERE
		i.RangeMarker = 1
		AND
		i.StartDate > @MaxFull
		AND
		o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID
	ORDER BY
		o.Created DESC

	------------------------------------------------------------------------------
	--
	-- Create temp table for storing results.
	--
	------------------------------------------------------------------------------

	-- Kinda create table
	SELECT
		TheMonth,
		NextMonth = TheMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		AverageItemsPerOrderDenominator,
		AverageItemsPerOrderNumerator,
		AverageSumOfOrderDenominator,
		AverageSumOfOrderNumerator,
		CancelledOrdersCount,
		NumOfOrders,
		OrdersCancellationRateDenominator,
		OrdersCancellationRateNumerator,
		TotalItemsOrdered,
		TotalSumOfOrders
	INTO
		#months
	FROM
		EbayAggregation
	WHERE
		1 = 0

	------------------------------------------------------------------------------
	--
	-- Extract single months from the relevant transactions.
	--
	------------------------------------------------------------------------------

	INSERT INTO #months (
		TheMonth,
		NextMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		AverageItemsPerOrderDenominator,
		AverageItemsPerOrderNumerator,
		AverageSumOfOrderDenominator,
		AverageSumOfOrderNumerator,
		CancelledOrdersCount,
		NumOfOrders,
		OrdersCancellationRateDenominator,
		OrdersCancellationRateNumerator,
		TotalItemsOrdered,
		TotalSumOfOrders
	)
	SELECT DISTINCT
		dbo.udfMonthStart(StartDate),
		'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value in the next query.
		@HistoryID,
		0, -- Turnover,
		0, -- AverageItemsPerOrderDenominator,
		0, -- AverageItemsPerOrderNumerator,
		0, -- AverageSumOfOrderDenominator,
		0, -- AverageSumOfOrderNumerator,
		0, -- CancelledOrdersCount,
		0, -- NumOfOrders,
		0, -- OrdersCancellationRateDenominator,
		0, -- OrdersCancellationRateNumerator,
		0, -- TotalItemsOrdered,
		0  -- TotalSumOfOrders
	FROM
		#order_items

	------------------------------------------------------------------------------

	UPDATE #months SET
		NextMonth = DATEADD(second, -1, DATEADD(month, 1, TheMonth))

	------------------------------------------------------------------------------
	--
	-- Calculate Turnover, TotalSumOfOrders, and AverageSumOfOrderNumerator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		Turnover = ISNULL(d.Value, 0),
		TotalSumOfOrders = ISNULL(d.Value, 0),
		AverageSumOfOrderNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.Revenue, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.StartDate BETWEEN im.TheMonth AND im.NextMonth
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfOrders, AverageItemsPerOrderDenominator,
	--           AverageSumOfOrderDenominator, and OrdersCancellationRateDenominator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfOrders = ISNULL(d.Value, 0),
		AverageItemsPerOrderDenominator = ISNULL(d.Value, 0),
		AverageSumOfOrderDenominator = ISNULL(d.Value, 0),
		OrdersCancellationRateDenominator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.Transactions, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.StartDate BETWEEN im.TheMonth AND im.NextMonth
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalItemsOrdered and AverageItemsPerOrderNumerator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalItemsOrdered = ISNULL(d.Value, 0),
		AverageItemsPerOrderNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.ItemsSold, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.StartDate BETWEEN im.TheMonth AND im.NextMonth
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate CancelledOrdersCount and OrdersCancellationRateNumerator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		CancelledOrdersCount = ISNULL(d.Value, 0),
		OrdersCancellationRateNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.Listings - i.Successful, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.StartDate BETWEEN im.TheMonth AND im.NextMonth
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TopCategories
	--
	------------------------------------------------------------------------------

	-- 1. Select all the full months of the marketplace.

	SELECT
		i.Id
	INTO
		#all_ids
	FROM
		MP_TeraPeakOrderItem i
		INNER JOIN MP_TeraPeakOrder o ON i.TeraPeakOrderId = o.Id
	WHERE
		i.RangeMarker = 0
		AND
		o.CustomerMarketPlaceId = @MpID

	-- 2. Append the last partial month of the marketplace.

	INSERT INTO #all_ids (Id)
	SELECT
		Id
	FROM
		#order_items
	WHERE
		RangeMarker = 1

	-- 3. Create statistics.

	SELECT
		s.CategoryId,
		SUM(s.Listings) AS Listings
	INTO
		#cat
	FROM
		MP_TeraPeakCategoryStatistics s
		INNER JOIN #all_ids a ON s.OrderItemId = a.Id
	GROUP BY
		s.CategoryId

	------------------------------------------------------------------------------
	--
	-- At this point table #months contains new aggregated data and table #cat
	-- contains updated category statistics.
	--
	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	UPDATE EbayAggregation SET
		IsActive = 0
	FROM
		EbayAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #months m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO EbayAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		AverageItemsPerOrderDenominator,
		AverageItemsPerOrderNumerator,
		AverageSumOfOrderDenominator,
		AverageSumOfOrderNumerator,
		CancelledOrdersCount,
		NumOfOrders,
		OrdersCancellationRateDenominator,
		OrdersCancellationRateNumerator,
		TotalItemsOrdered,
		TotalSumOfOrders
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		AverageItemsPerOrderDenominator,
		AverageItemsPerOrderNumerator,
		AverageSumOfOrderDenominator,
		AverageSumOfOrderNumerator,
		CancelledOrdersCount,
		NumOfOrders,
		OrdersCancellationRateDenominator,
		OrdersCancellationRateNumerator,
		TotalItemsOrdered,
		TotalSumOfOrders
	FROM
		#months

	------------------------------------------------------------------------------

	UPDATE EbayAggregationCategories SET
		IsActive = 0
	FROM
		EbayAggregationCategories a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO EbayAggregationCategories (
		CustomerMarketPlaceUpdatingHistoryID,
		IsActive,
		CategoryID,
		Listings
	)
	SELECT
		@HistoryID,
		1, -- IsActive
		CategoryId,
		Listings
	FROM
		#cat

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------
	--
	-- Clean up.
	--
	------------------------------------------------------------------------------

	DROP TABLE #cat
	DROP TABLE #all_ids
	DROP TABLE #months
	DROP TABLE #order_items
END
GO
