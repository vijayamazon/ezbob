IF OBJECT_ID('UpdateMpTotalsChaGra') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsChaGra AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsChaGra
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

	EXECUTE GetLastCustomerMarketplaceUpdatingHistoryID 'Channel Grabber', @MpID, @HistoryID, @LastHistoryID OUTPUT

	IF @LastHistoryID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	CREATE TABLE #chagra_id (
		NativeOrderId NVARCHAR(300),
		Id INT
	)

	-- Step 1. Find all the relevant internal ids.

	IF @HistoryID IS NULL
	BEGIN
		INSERT INTO #chagra_id (NativeOrderId, Id)
		SELECT
			i.NativeOrderId,
			MAX(i.Id) AS Id
		FROM
			MP_ChannelGrabberOrder o
			INNER JOIN MP_ChannelGrabberOrderItem i ON o.Id = i.OrderId
		WHERE
			o.CustomerMarketPlaceId = @MpID
		GROUP BY
			i.NativeOrderId
	END
	ELSE BEGIN
		-- 1. Load list of months that are going to be updated.
		SELECT DISTINCT
			TheMonth = dbo.udfMonthStart(i.PaymentDate),
			NextMonth = CONVERT(DATETIME, NULL)
		INTO
			#month_list
		FROM
			MP_ChannelGrabberOrder o
			INNER JOIN MP_ChannelGrabberOrderItem i ON o.Id = i.OrderId
		WHERE
			o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID

		-- 2. Establish one month range for every row, this completes list of months that are going to be updated.
		UPDATE #month_list SET
			NextMonth = dbo.udfMonthEnd(TheMonth)

		-- 3. Select internal ids of order items that belong to the months that are going to be updated.
		--    Apply uniqueness rule.
		INSERT INTO #chagra_id (NativeOrderId, Id)
		SELECT
			i.NativeOrderId,
			MAX(i.Id) AS Id
		FROM
			MP_ChannelGrabberOrder o
			INNER JOIN MP_ChannelGrabberOrderItem i
				ON o.Id = i.OrderId
			INNER JOIN #month_list ml
				ON i.PaymentDate BETWEEN ml.TheMonth AND ml.NextMonth
		WHERE
			o.CustomerMarketPlaceId = @MpID
		GROUP BY
			i.NativeOrderId

		-- 4. Clean up for this step.
		DROP TABLE #month_list
	END

	------------------------------------------------------------------------------

	-- Step 2. Load relevant data to temp table.

	SELECT
		TotalCost = i.TotalCost* dbo.udfGetCurrencyRate(i.PurchaseDate, i.CurrencyCode),
		i.PaymentDate,
		IsExpense = ISNULL(i.IsExpense, 0)
	INTO
		#order_items
	FROM
		MP_ChannelGrabberOrderItem i
		INNER JOIN #chagra_id o ON i.Id = o.Id

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
		AverageSumOfExpensesDenominator,
		AverageSumOfExpensesNumerator,
		AverageSumOfOrdersDenominator,
		AverageSumOfOrdersNumerator,
		NumOfExpenses,
		NumOfOrders,
		TotalSumOfExpenses,
		TotalSumOfOrders
	INTO
		#months
	FROM
		ChannelGrabberAggregation
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
		AverageSumOfExpensesDenominator,
		AverageSumOfExpensesNumerator,
		AverageSumOfOrdersDenominator,
		AverageSumOfOrdersNumerator,
		NumOfExpenses,
		NumOfOrders,
		TotalSumOfExpenses,
		TotalSumOfOrders
	)
	SELECT DISTINCT
		dbo.udfMonthStart(PaymentDate),
		'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value in the next query.
		@LastHistoryID,
		0, -- Turnover,
		0, -- AverageSumOfExpensesDenominator,
		0, -- AverageSumOfExpensesNumerator,
		0, -- AverageSumOfOrdersDenominator,
		0, -- AverageSumOfOrdersNumerator,
		0, -- NumOfExpenses,
		0, -- NumOfOrders,
		0, -- TotalSumOfExpenses,
		0  -- TotalSumOfOrders
	FROM
		#order_items

	------------------------------------------------------------------------------

	UPDATE #months SET
		NextMonth = DATEADD(second, -1, DATEADD(month, 1, TheMonth))

	------------------------------------------------------------------------------
	--
	-- Calculate Turnover, TotalSumOfOrders, and AverageSumOfOrdersNumerator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		Turnover = ISNULL(d.Value, 0),
		TotalSumOfOrders = ISNULL(d.Value, 0),
		AverageSumOfOrdersNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.TotalCost, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.PaymentDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsExpense = 0
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfExpenses and AverageSumOfExpensesNumerator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfExpenses = ISNULL(d.Value, 0),
		AverageSumOfExpensesNumerator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.TotalCost, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.PaymentDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsExpense = 1
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfExpenses and AverageSumOfExpensesDenominator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfExpenses = ISNULL(d.Value, 0),
		AverageSumOfExpensesDenominator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.PaymentDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsExpense = 1
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfOrders and AverageSumOfOrdersDenominator.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfOrders = ISNULL(d.Value, 0),
		AverageSumOfOrdersDenominator = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.PaymentDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsExpense = 0
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

	UPDATE ChannelGrabberAggregation SET
		IsActive = 0
	FROM
		ChannelGrabberAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #months m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO ChannelGrabberAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		AverageSumOfExpensesDenominator,
		AverageSumOfExpensesNumerator,
		AverageSumOfOrdersDenominator,
		AverageSumOfOrdersNumerator,
		NumOfExpenses,
		NumOfOrders,
		TotalSumOfExpenses,
		TotalSumOfOrders
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		AverageSumOfExpensesDenominator,
		AverageSumOfExpensesNumerator,
		AverageSumOfOrdersDenominator,
		AverageSumOfOrdersNumerator,
		NumOfExpenses,
		NumOfOrders,
		TotalSumOfExpenses,
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
	DROP TABLE #chagra_id
END
GO
