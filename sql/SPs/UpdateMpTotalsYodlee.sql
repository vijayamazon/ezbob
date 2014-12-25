-- Depends on type YodleeOrderItems. Type's file drops this stored procedure.

IF OBJECT_ID('UpdateMpTotalsYodlee') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsYodlee AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsYodlee
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @MpID INT

	EXECUTE GetMarketplaceFromHistoryID 'Yodlee', @HistoryID, @MpID OUTPUT

	IF @MpID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	-- Step 1. Detect bank type: parsed or linked (because we don't trust so much
	-- in parsed banks).

	------------------------------------------------------------------------------

	DECLARE @ParsedBank BIT = ISNULL((
		SELECT
			CASE mp.DisplayName WHEN 'ParsedBank' THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END
		FROM
			MP_CustomerMarketPlace mp
		WHERE
			mp.Id = @MpID
	), 1)

	------------------------------------------------------------------------------

	-- Step 2. Load relevant orders (not order items!).

	------------------------------------------------------------------------------

	CREATE TABLE #orders (Id INT)

	------------------------------------------------------------------------------

	INSERT INTO #orders
	SELECT
		o.Id
	FROM
		MP_YodleeOrder o
	WHERE
		o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID

	------------------------------------------------------------------------------

	-- Step 3. Select relevant order items.
	-- For parsed bank: all corresponding to #orders.
	-- For linked bank: corresponding to #orders, within same months, and distinct
	-- by srcElementId.

	------------------------------------------------------------------------------

	DECLARE @order_items YodleeOrderItems

	------------------------------------------------------------------------------

	IF @ParsedBank = 1
	BEGIN -- order items for parsed bank
		INSERT INTO @order_items(
			id,
			srcElementId,
			transactionBaseTypeId,
			EzbobCategory,
			transactionAmount,
			theDate
		)
		SELECT
			t.id,
			t.srcElementId,
			t.transactionBaseTypeId,
			t.EzbobCategory,
			t.transactionAmount * dbo.udfGetCurrencyRate(ISNULL(t.postDate, t.transactionDate), t.transactionAmountCurrency),
			ISNULL(t.postDate, t.transactionDate)
		FROM
			MP_YodleeOrderItemBankTransaction t
			INNER JOIN MP_YodleeOrderItem i ON t.OrderItemId = i.Id
			INNER JOIN #orders o ON i.OrderId = o.Id
		WHERE
			ISNULL(t.isSeidMod, 0) = 0
			AND
			t.transactionStatusId = 1 -- Posted
			AND
			ISNULL(t.postDate, t.transactionDate) IS NOT NULL
	END -- order items for parsed bank
	ELSE BEGIN -- order items for linked bank
		-- Find relevant months.
		SELECT DISTINCT
			TheMonth = dbo.udfMonthStart(ISNULL(t.postDate, t.transactionDate))
		INTO
			#m
		FROM
			MP_YodleeOrderItemBankTransaction t
			INNER JOIN MP_YodleeOrderItem i ON t.OrderItemId = i.Id
			INNER JOIN #orders o ON i.OrderId = o.Id
		WHERE
			ISNULL(t.isSeidMod, 0) = 0
			AND
			t.transactionStatusId = 1 -- Posted
			AND
			ISNULL(t.postDate, t.transactionDate) IS NOT NULL

		-- Find the last transaction in every class of equivalency.
		SELECT
			id = MAX(t.id),
			t.srcElementId
		INTO
			#ids
		FROM
			MP_YodleeOrderItemBankTransaction t
			INNER JOIN MP_YodleeOrderItem i ON t.OrderItemId = i.Id
			INNER JOIN MP_YodleeOrder o
				ON i.OrderId = o.Id
				AND o.CustomerMarketPlaceId = @MpID
			INNER JOIN #m
				ON dbo.udfMonthStart(ISNULL(postDate, transactionDate)) = #m.TheMonth
		WHERE
			ISNULL(t.isSeidMod, 0) = 0
			AND
			t.transactionStatusId = 1 -- Posted
			AND
			ISNULL(postDate, transactionDate) IS NOT NULL
		GROUP BY
			t.srcElementId

		-- Load relevant transactions to temp table.
		INSERT INTO @order_items(
			id,
			srcElementId,
			transactionBaseTypeId,
			EzbobCategory,
			transactionAmount,
			theDate
		)
		SELECT
			t.id,
			t.srcElementId,
			t.transactionBaseTypeId,
			t.EzbobCategory,
			t.transactionAmount * dbo.udfGetCurrencyRate(ISNULL(t.postDate, t.transactionDate), t.transactionAmountCurrency),
			ISNULL(t.postDate, t.transactionDate)
		FROM
			MP_YodleeOrderItemBankTransaction t
			INNER JOIN #ids ON t.id = #ids.id

		-- Local cleanup.
		DROP TABLE #ids
		DROP TABLE #m
	END -- order items for linked bank

	------------------------------------------------------------------------------
	--
	-- Create temp tables for storing results.
	--
	------------------------------------------------------------------------------

	-- Kinda create table
	SELECT
		TheMonth,
		NextMonth = TheMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		NumberOfTransactions,
		TotalExpense,
		TotalIncome,
		NetCashFlow
	INTO
		#months
	FROM
		YodleeAggregation
	WHERE
		1 = 0

	------------------------------------------------------------------------------

	-- Kinda create table
	SELECT
		TheMonth,
		NextMonth = TheMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		MainGroupID,
		SubGroupID,
		BaseTypeID,
		Value
	INTO
		#group_months
	FROM
		YodleeGroupAggregation
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
		NumberOfTransactions,
		TotalExpense,
		TotalIncome,
		NetCashFlow
	)
	SELECT DISTINCT
		dbo.udfMonthStart(theDate),
		'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value in the next query.
		@HistoryID,
		0, -- Turnover,
		0, -- NumberOfTransactions,
		0, -- TotalExpense,
		0, -- TotalIncome,
		0  -- NetCashFlow
	FROM
		@order_items

	------------------------------------------------------------------------------

	UPDATE #months SET
		NextMonth = DATEADD(second, -1, DATEADD(month, 1, TheMonth))

	------------------------------------------------------------------------------

	INSERT INTO #group_months(
		TheMonth,
		NextMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		MainGroupID,
		SubGroupID,
		BaseTypeID,
		Value
	)
	SELECT
		m.TheMonth,
		m.NextMonth,
		m.CustomerMarketPlaceUpdatingHistoryID,
		null, -- MainGroupID
		sg.SubGroupID,
		t.BaseTypeID,
		0 -- Value
	FROM
		#months m,
		MP_YodleeSubGroups sg
		INNER JOIN MP_YodleeMainGroups mg ON sg.MainGroupID = mg.MainGroupID
		INNER JOIN MP_YodleeTransactionBaseTypes t
			ON sg.BaseTypeID IS NULL OR sg.BaseTypeID = t.BaseTypeID

	------------------------------------------------------------------------------
	--
	-- Calculate NumberOfTransactions
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumberOfTransactions = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN @order_items i
					ON i.theDate BETWEEN im.TheMonth AND im.NextMonth
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------

	DECLARE @Credit INT = (SELECT BaseTypeID FROM MP_YodleeTransactionBaseTypes WHERE BaseTypeName = 'credit')
	DECLARE @Debit  INT = (SELECT BaseTypeID FROM MP_YodleeTransactionBaseTypes WHERE BaseTypeName = 'debit')

	------------------------------------------------------------------------------
	--
	-- Calculate TotalIncome and TotalExpense
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalIncome = dbo.udfYodleeFormula(@order_items, TheMonth, NextMonth, NULL, @Credit),
		TotalExpense = dbo.udfYodleeFormula(@order_items, TheMonth, NextMonth, NULL, @Debit)

	------------------------------------------------------------------------------

	UPDATE #months SET
		NetCashFlow = TotalIncome - TotalExpense

	------------------------------------------------------------------------------
	--
	-- Calculate sub-group totals.
	--
	------------------------------------------------------------------------------

	UPDATE #group_months SET
		Value = dbo.udfYodleeFormula(@order_items, TheMonth, NextMonth, SubGroupID, BaseTypeID)

	------------------------------------------------------------------------------
	--
	-- Calculate group totals.
	--
	------------------------------------------------------------------------------

	INSERT INTO #group_months(
		TheMonth,
		NextMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		MainGroupID,
		SubGroupID,
		BaseTypeID,
		Value
	)
	SELECT
		m.TheMonth,
		m.NextMonth,
		m.CustomerMarketPlaceUpdatingHistoryID,
		sg.MainGroupID,
		NULL, -- SubGroupID
		m.BaseTypeID,
		SUM(m.Value)
	FROM
		#group_months m
		INNER JOIN MP_YodleeSubGroups sg ON m.SubGroupID = sg.SubGroupID
	GROUP BY
		m.TheMonth,
		m.NextMonth,
		m.CustomerMarketPlaceUpdatingHistoryID,
		sg.MainGroupID,
		m.BaseTypeID

	------------------------------------------------------------------------------
	--
	-- Calculate Turnover.
	--
	------------------------------------------------------------------------------

	IF @ParsedBank = 0
	BEGIN
		UPDATE #months SET
			Turnover = ISNULL(gm.Value, 0)
		FROM
			#months m
			INNER JOIN #group_months gm
				ON m.TheMonth = gm.TheMonth
				AND gm.BaseTypeID = @Credit
			INNER JOIN MP_YodleeMainGroups y
				ON gm.MainGroupID = y.MainGroupID
				AND y.MainGroupName = 'Revenues'
	END

	------------------------------------------------------------------------------
	--
	-- At this point table #months contains new data.
	--
	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------
	IF @ParsedBank = 1
	BEGIN
		UPDATE YodleeAggregation SET
			IsActive = 0
		FROM
			YodleeAggregation a
			INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
				ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
				AND h.CustomerMarketplaceID = @MpID
		WHERE
			a.IsActive = 1
	END
	ELSE BEGIN
		UPDATE YodleeAggregation SET
			IsActive = 0
		FROM
			YodleeAggregation a
			INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
				ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
				AND h.CustomerMarketplaceID = @MpID
			INNER JOIN #months m ON a.TheMonth = m.TheMonth
		WHERE
			a.IsActive = 1
	END

	------------------------------------------------------------------------------

	INSERT INTO YodleeAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		NumberOfTransactions,
		TotalExpense,
		TotalIncome,
		NetCashFlow
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		NumberOfTransactions,
		TotalExpense,
		TotalIncome,
		NetCashFlow
	FROM
		#months

	------------------------------------------------------------------------------

	-- Data removing (i.e. marking it IsActive = 0) is done with the help of
	-- #months table because #group_months contains all the same months several
	-- times (once for each (sub-)group).
	
	IF @ParsedBank = 1
	BEGIN
		UPDATE YodleeGroupAggregation SET
			IsActive = 0
		FROM
			YodleeGroupAggregation a
			INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
				ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
				AND h.CustomerMarketplaceID = @MpID
		WHERE
			a.IsActive = 1
	END
	ELSE BEGIN
		UPDATE YodleeGroupAggregation SET
			IsActive = 0
		FROM
			YodleeGroupAggregation a
			INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
				ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
				AND h.CustomerMarketplaceID = @MpID
			INNER JOIN #months m ON a.TheMonth = m.TheMonth
		WHERE
			a.IsActive = 1
	END

	------------------------------------------------------------------------------

	INSERT INTO YodleeGroupAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		MainGroupID,
		SubGroupID,
		BaseTypeID,
		Value
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		MainGroupID,
		SubGroupID,
		BaseTypeID,
		Value
	FROM
		#group_months

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------
	--
	-- Cleanup.
	--
	------------------------------------------------------------------------------

	DROP TABLE #group_months
	DROP TABLE #months
	DROP TABLE #orders
END
GO
