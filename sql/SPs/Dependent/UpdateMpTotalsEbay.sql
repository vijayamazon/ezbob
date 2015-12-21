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

	;WITH relevant_months AS ( -- update all the months that have transactions in this fetch event
		SELECT DISTINCT
			TheMonth = dbo.udfMonthStart(oi.CreatedTime)
		FROM
			MP_EbayOrderItem oi
			INNER JOIN MP_EbayOrder o
				ON oi.OrderId = o.Id
				AND o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID
		WHERE
			oi.CreatedTime IS NOT NULL
	), transaction_unificator AS ( -- process only unique transactions by eBayTransactionId
		SELECT
			TransactionID = t.Id,
			Pos = ROW_NUMBER() OVER (
				PARTITION BY t.eBayTransactionId
				ORDER BY
					o.Created DESC,
					o.Id DESC,
					oi.OrderStatus DESC,
					oi.CheckoutStatus ASC,
					oi.AmountPaidAmount DESC
			)
		FROM
			MP_EbayTransaction t
			INNER JOIN MP_EbayOrderItem oi ON t.OrderItemId = oi.Id
			INNER JOIN MP_EbayOrder o
				ON oi.OrderId = o.Id
				AND o.CustomerMarketPlaceId = @MpID
			INNER JOIN relevant_months rm ON rm.TheMonth = dbo.udfMonthStart(oi.CreatedTime)
	), raw_order_data AS ( -- gather transaction and order item data
		SELECT
			OrderItemID = oi.Id,
			OrderTime = oi.CreatedTime,
			OrderStatus = oi.OrderStatus,
			OrderAmount = oi.TotalAmount * dbo.udfGetCurrencyRate(oi.CreatedTime, oi.TotalCurrency),
			TransactionID = t.Id,
			ItemCount = t.QuantityPurchased
		FROM
			MP_EbayTransaction t
			INNER JOIN transaction_unificator tu ON tu.TransactionID = t.Id AND tu.Pos = 1
			INNER JOIN MP_EbayOrderItem oi ON t.OrderItemId = oi.Id
			INNER JOIN MP_EbayOrder o ON oi.OrderId = o.Id
	) SELECT -- group data by order item id
		o.OrderItemID,
		o.OrderTime,
		o.OrderStatus,
		o.OrderAmount,
		ItemCount = SUM(o.ItemCount)
	INTO
		#order_items
	FROM
		raw_order_data o
	GROUP BY
		o.OrderItemID,
		o.OrderTime,
		o.OrderStatus,
		o.OrderAmount

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
		dbo.udfMonthStart(OrderTime),
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
				Value = SUM(ISNULL(i.OrderAmount, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderTime BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus IN ('Completed', 'Authenticated', 'Shipped')
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfOrders, AverageItemsPerOrderDenominator,
	-- AverageSumOfOrderDenominator, and OrdersCancellationRateDenominator.
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
				Value = COUNT(DISTINCT i.OrderItemID)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderTime BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus IN ('Completed', 'Authenticated', 'Shipped')
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
				Value = SUM(ISNULL(i.ItemCount, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderTime BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus IN ('Completed', 'Authenticated', 'Shipped')
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
				Value = COUNT(DISTINCT i.OrderItemID)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderTime BETWEEN im.TheMonth AND im.NextMonth
					AND i.OrderStatus IN ('Cancelled')
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- At this point table #months contains new aggregated data.
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
