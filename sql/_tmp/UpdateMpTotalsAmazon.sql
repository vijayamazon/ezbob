IF OBJECT_ID('UpdateMpTotalsAmazon') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsAmazon AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsAmazon
@MpID INT,
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------
	--
	-- Find last history id (for backfills).
	--
	------------------------------------------------------------------------------

	DECLARE @LastHistoryID INT

	EXECUTE GetLastCustomerMarketplaceUpdatingHistoryID 'Amazon', @MpID, @HistoryID, @LastHistoryID OUTPUT

	IF @LastHistoryID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	SELECT
		i.Id,
		i.OrderStatus,
		i.NumberOfItemsShipped,
		i.PurchaseDate,
		i.LastUpdateDate,
		OrderTotal = i.OrderTotal * dbo.udfGetCurrencyRate(i.LastUpdateDate, i.OrderTotalCurrency)
	INTO
		#order_items
	FROM
		MP_AmazonOrder o
		INNER JOIN MP_AmazonOrderItem i
			ON o.Id = i.AmazonOrderId
	WHERE
		(
			@HistoryID IS NOT NULL
			AND
			o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID
		)
		OR
		o.CustomerMarketPlaceId = @MpID

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
		AmazonAggregation
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
		dbo.udfMonthStart(LastUpdateDate),
		'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value in the next query.
		@LastHistoryID,
		0, -- Turnover
		0, -- AverageItemsPerOrderDenominator
		0, -- AverageItemsPerOrderNumerator
		0, -- AverageSumOfOrderDenominator
		0, -- AverageSumOfOrderNumerator
		0, -- CancelledOrdersCount
		0, -- NumOfOrders
		0, -- OrdersCancellationRateDenominator
		0, -- OrdersCancellationRateNumerator
		0, -- TotalItemsOrdered
		0  -- TotalSumOfOrders
	FROM
		#order_items

	------------------------------------------------------------------------------

	UPDATE #months SET
		NextMonth = DATEADD(second, -1, DATEADD(month, 1, TheMonth))

	------------------------------------------------------------------------------
	--
	-- Calculate turnover.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		Turnover = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.OrderTotal, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.LastUpdateDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus = 'Shipped'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfOrders
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfOrders = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.LastUpdateDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus = 'Shipped'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate CancelledOrdersCount
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		CancelledOrdersCount = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.LastUpdateDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus = 'Canceled'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalItemsOrdered
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalItemsOrdered = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.NumberOfItemsShipped, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.LastUpdateDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus = 'Shipped'
					AND i.NumberOfItemsShipped IS NOT NULL
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfOrders
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfOrders = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.OrderTotal, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.LastUpdateDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus = 'Shipped'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate OrdersCancellationRate (must be after NumOfOrders)
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		OrdersCancellationRateNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.LastUpdateDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus = 'Canceled'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------

	UPDATE #months SET
		OrdersCancellationRateDenominator = NumOfOrders,
		AverageItemsPerOrderDenominator = NumOfOrders,
		AverageItemsPerOrderNumerator = TotalItemsOrdered,
		AverageSumOfOrderDenominator = NumOfOrders,
		AverageSumOfOrderNumerator = TotalSumOfOrders

	------------------------------------------------------------------------------
	--
	-- At this point table #months contains new data.
	--
	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	UPDATE AmazonAggregation SET
		IsActive = 0
	FROM
		AmazonAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #months m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO AmazonAggregation (
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

	COMMIT TRANSACTION

	------------------------------------------------------------------------------
	--
	-- Clean up.
	--
	------------------------------------------------------------------------------

	DROP TABLE #months
	DROP TABLE #order_items
END
GO
