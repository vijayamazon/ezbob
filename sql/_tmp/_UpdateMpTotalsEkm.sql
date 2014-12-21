IF OBJECT_ID('UpdateMpTotalsEkm') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsEkm AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsEkm
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

	EXECUTE GetLastCustomerMarketplaceUpdatingHistoryID 'EKM', @MpID, @HistoryID, @LastHistoryID OUTPUT

	IF @LastHistoryID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	CREATE TABLE #ekm_id (
		OrderNumber NVARCHAR(300),
		Id INT
	)

	------------------------------------------------------------------------------

	-- Step 1. Find all the relevant internal ids.
	
	IF @HistoryID IS NULL
	BEGIN
		INSERT INTO #ekm_id(OrderNumber, Id)
		SELECT
			i.OrderNumber,
			MAX(i.Id) AS Id
		FROM
			MP_EkmOrder o
			INNER JOIN MP_EkmOrderItem i ON o.Id = i.OrderId
		WHERE
			o.CustomerMarketPlaceId = @MpID
		GROUP BY
			i.OrderNumber
	END
	ELSE BEGIN
		-- 1. Load list of months that are going to be updated.
		SELECT DISTINCT
			TheMonth = dbo.udfMonthStart(i.OrderDate),
			NextMonth = CONVERT(DATETIME, NULL)
		INTO
			#month_list
		FROM
			MP_EkmOrder o
			INNER JOIN MP_EkmOrderItem i ON o.Id = i.OrderId
		WHERE
			o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID

		-- 2. Establish one month range for every row, this completes list of months that are going to be updated.
		UPDATE #month_list SET
			NextMonth = dbo.udfMonthEnd(TheMonth)

		-- 3. Select internal ids of order items that belong to the months that are going to be updated.
		--    Apply uniqueness rule.
		INSERT INTO #ekm_id(OrderNumber, Id)
		SELECT
			i.OrderNumber,
			MAX(i.Id) AS Id
		FROM
			MP_EkmOrder o
			INNER JOIN MP_EkmOrderItem i
				ON o.Id = i.OrderId
			INNER JOIN #month_list ml
				ON i.OrderDate BETWEEN ml.TheMonth AND ml.NextMonth
		WHERE
			o.CustomerMarketPlaceId = @MpID
		GROUP BY
			i.OrderNumber

		-- 4. Clean up for this step.
		DROP TABLE #month_list
	END

	------------------------------------------------------------------------------

	-- Step 2. Load relevant data to temp table.

	SELECT
		i.TotalCost,
		i.OrderDate,
		OrderStatus = LOWER(LTRIM(RTRIM(i.OrderStatus))),
		IsCancelled = CONVERT(BIT, 0),
		IsCompleted = CONVERT(BIT, 0)
	INTO
		#order_items
	FROM
		MP_EkmOrderItem i
		INNER JOIN #ekm_id o ON i.Id = o.Id

	------------------------------------------------------------------------------

	-- Step 3. Mark cancelled and completed orders.

	UPDATE #order_items SET
		IsCancelled = CASE WHEN OrderStatus IN ('refunded', 'failed', 'cancelled') THEN 1 ELSE 0 END,
		IsCompleted = CASE WHEN OrderStatus IN ('dispatched', 'processing', 'pending', 'complete') THEN 1 ELSE 0 END

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
		AverageSumOfCancelledOrderDenominator,
		AverageSumOfCancelledOrderNumerator,
		AverageSumOfOrderDenominator,
		AverageSumOfOrderNumerator,
		AverageSumOfOtherOrderDenominator,
		AverageSumOfOtherOrderNumerator,
		CancellationRateDenominator,
		CancellationRateNumerator,
		NumOfCancelledOrders,
		NumOfOrders,
		NumOfOtherOrders,
		TotalSumOfCancelledOrders,
		TotalSumOfOrders,
		TotalSumOfOtherOrders
	INTO
		#months
	FROM
		EkmAggregation
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
		AverageSumOfCancelledOrderDenominator,
		AverageSumOfCancelledOrderNumerator,
		AverageSumOfOrderDenominator,
		AverageSumOfOrderNumerator,
		AverageSumOfOtherOrderDenominator,
		AverageSumOfOtherOrderNumerator,
		CancellationRateDenominator,
		CancellationRateNumerator,
		NumOfCancelledOrders,
		NumOfOrders,
		NumOfOtherOrders,
		TotalSumOfCancelledOrders,
		TotalSumOfOrders,
		TotalSumOfOtherOrders
	)
	SELECT DISTINCT
		dbo.udfMonthStart(PaymentDate),
		'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value in the next query.
		@LastHistoryID,
		0, -- Turnover,
		0, -- AverageSumOfCancelledOrderDenominator,
		0, -- AverageSumOfCancelledOrderNumerator,
		0, -- AverageSumOfOrderDenominator,
		0, -- AverageSumOfOrderNumerator,
		0, -- AverageSumOfOtherOrderDenominator,
		0, -- AverageSumOfOtherOrderNumerator,
		0, -- CancellationRateDenominator,
		0, -- CancellationRateNumerator,
		0, -- NumOfCancelledOrders,
		0, -- NumOfOrders,
		0, -- NumOfOtherOrders,
		0, -- TotalSumOfCancelledOrders,
		0, -- TotalSumOfOrders,
		0  -- TotalSumOfOtherOrders
	FROM
		#order_items

	------------------------------------------------------------------------------

	UPDATE #months SET
		NextMonth = DATEADD(second, -1, DATEADD(month, 1, TheMonth))

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfOrders, CancellationRateDenominator, AverageSumOfOrderDenominator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfOrders = ISNULL(d.Value, 0),
		CancellationRateDenominator = ISNULL(d.Value, 0),
		AverageSumOfOrderDenominator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsCancelled = 0
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate Turnover, TotalSumOfOrders and AverageSumOfOrderNumerator.
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
				Value = SUM(ISNULL(i.TotalCost, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsCancelled = 0
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfCancelledOrders, AverageSumOfCancelledOrderDenominator, and
	-- CancellationRateNumerator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfCancelledOrders = ISNULL(d.Value, 0),
		AverageSumOfCancelledOrderDenominator = ISNULL(d.Value, 0),
		CancellationRateNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsCancelled = 1
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfCancelledOrders and AverageSumOfCancelledOrderNumerator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfCancelledOrders = ISNULL(d.Value, 0),
		AverageSumOfCancelledOrderNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.TotalCost, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsCancelled = 1
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfOtherOrders and AverageSumOfOtherOrderDenominator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfOtherOrders = ISNULL(d.Value, 0),
		AverageSumOfOtherOrderDenominator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsCancelled = 0
					AND i.IsCompleted = 0
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfOtherOrders and AverageSumOfOtherOrderNumerator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfOtherOrders = ISNULL(d.Value, 0),
		AverageSumOfOtherOrderNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.TotalCost, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.OrderDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsCancelled = 0
					AND i.IsCompleted = 0
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- At this point table #months contains new data.
	--
	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	UPDATE EkmAggregation SET
		IsActive = 0
	FROM
		EkmAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #months m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO EkmAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		AverageSumOfCancelledOrderDenominator,
		AverageSumOfCancelledOrderNumerator,
		AverageSumOfOrderDenominator,
		AverageSumOfOrderNumerator,
		AverageSumOfOtherOrderDenominator,
		AverageSumOfOtherOrderNumerator,
		CancellationRateDenominator,
		CancellationRateNumerator,
		NumOfCancelledOrders,
		NumOfOrders,
		NumOfOtherOrders,
		TotalSumOfCancelledOrders,
		TotalSumOfOrders,
		TotalSumOfOtherOrders
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		AverageSumOfCancelledOrderDenominator,
		AverageSumOfCancelledOrderNumerator,
		AverageSumOfOrderDenominator,
		AverageSumOfOrderNumerator,
		AverageSumOfOtherOrderDenominator,
		AverageSumOfOtherOrderNumerator,
		CancellationRateDenominator,
		CancellationRateNumerator,
		NumOfCancelledOrders,
		NumOfOrders,
		NumOfOtherOrders,
		TotalSumOfCancelledOrders,
		TotalSumOfOrders,
		TotalSumOfOtherOrders
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
	DROP TABLE #ekm_id
END
GO
