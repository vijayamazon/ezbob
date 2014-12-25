IF OBJECT_ID('UpdateMpTotalsFreeAgent') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsFreeAgent AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsFreeAgent
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @MpID INT

	EXECUTE GetMarketplaceFromHistoryID 'FreeAgent', @HistoryID, @MpID OUTPUT

	IF @MpID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	CREATE TABLE #fa_id (
		url NVARCHAR(250),
		Id INT,
		IsInvoice BIT
	)

	------------------------------------------------------------------------------

	-- Step 1. Find all the relevant internal ids.
	
	CREATE TABLE #month_list (
		TheMonth DATETIME,
		NextMonth DATETIME
	)

	-- 1. Load list of months that are going to be updated.
	INSERT INTO #month_list(TheMonth, NextMonth)
	SELECT DISTINCT
		dbo.udfMonthStart(i.dated_on),
		CONVERT(DATETIME, NULL)
	FROM
		MP_FreeAgentRequest o
		INNER JOIN MP_FreeAgentInvoice i ON o.Id = i.RequestId
	WHERE
		o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID
	UNION
	SELECT DISTINCT
		dbo.udfMonthStart(i.dated_on),
		CONVERT(DATETIME, NULL)
	FROM
		MP_FreeAgentRequest o
		INNER JOIN MP_FreeAgentExpense i ON o.Id = i.RequestId
	WHERE
		o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID

	-- 2. Establish one month range for every row, this completes list of months that are going to be updated.
	UPDATE #month_list SET
		NextMonth = dbo.udfMonthEnd(TheMonth)

	-- 3. Select internal ids of order items that belong to the months that are going to be updated.
	--    Apply uniqueness rule.
	INSERT INTO #fa_id(url, Id, IsInvoice)
	SELECT
		i.url,
		MAX(i.Id) AS Id,
		1 -- IsInvoice
	FROM
		MP_FreeAgentRequest o
		INNER JOIN MP_FreeAgentInvoice i
			ON o.Id = i.RequestId
		INNER JOIN #month_list ml
			ON i.dated_on BETWEEN ml.TheMonth AND ml.NextMonth
	GROUP BY
		i.url

	INSERT INTO #fa_id(url, Id, IsInvoice)
	SELECT
		i.url,
		MAX(i.Id) AS Id,
		0 -- IsInvoice
	FROM
		MP_FreeAgentRequest o
		INNER JOIN MP_FreeAgentExpense i
			ON o.Id = i.RequestId
		INNER JOIN #month_list ml
			ON i.dated_on BETWEEN ml.TheMonth AND ml.NextMonth
	GROUP BY
		i.url

	-- 4. Clean up for this step.
	DROP TABLE #month_list

	------------------------------------------------------------------------------

	-- Step 2. Load relevant data to temp table.

	CREATE TABLE #order_items (
		is_invoice BIT,
		value NUMERIC(18, 2),
		dated_on DATETIME,
		status_or_cat NVARCHAR(250)
	)

	------------------------------------------------------------------------------

	INSERT INTO #order_items (is_invoice, value, dated_on, status_or_cat)
	SELECT
		f.IsInvoice,
		i.currency * dbo.udfGetCurrencyRate(i.dated_on, i.currency),
		i.dated_on,
		i.status
	FROM
		MP_FreeAgentInvoice i
		INNER JOIN #fa_id f
			ON i.Id = f.Id
			AND f.IsInvoice = 1

	------------------------------------------------------------------------------

	INSERT INTO #order_items (is_invoice, value, dated_on, status_or_cat)
	SELECT
		f.IsInvoice,
		i.gross_value * dbo.udfGetCurrencyRate(i.dated_on, i.currency),
		i.dated_on,
		c.category_group
	FROM
		MP_FreeAgentExpense i
		INNER JOIN MP_FreeAgentExpenseCategory c
			ON i.CategoryId = c.Id
		INNER JOIN #fa_id f
			ON i.Id = f.Id
			AND f.IsInvoice = 0

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
		NumOfExpenses,
		NumOfOrders,
		SumOfAdminExpensesCategory,
		SumOfCostOfSalesExpensesCategory,
		SumOfDraftInvoices,
		SumOfGeneralExpensesCategory,
		SumOfOpenInvoices,
		SumOfOverdueInvoices,
		SumOfPaidInvoices,
		TotalSumOfExpenses,
		TotalSumOfOrders
	INTO
		#months
	FROM
		FreeAgentAggregation
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
		NumOfExpenses,
		NumOfOrders,
		SumOfAdminExpensesCategory,
		SumOfCostOfSalesExpensesCategory,
		SumOfDraftInvoices,
		SumOfGeneralExpensesCategory,
		SumOfOpenInvoices,
		SumOfOverdueInvoices,
		SumOfPaidInvoices,
		TotalSumOfExpenses,
		TotalSumOfOrders
	)
	SELECT DISTINCT
		dbo.udfMonthStart(dated_on),
		'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value in the next query.
		@HistoryID,
		0, -- Turnover,
		0, -- NumOfExpenses,
		0, -- NumOfOrders,
		0, -- SumOfAdminExpensesCategory,
		0, -- SumOfCostOfSalesExpensesCategory,
		0, -- SumOfDraftInvoices,
		0, -- SumOfGeneralExpensesCategory,
		0, -- SumOfOpenInvoices,
		0, -- SumOfOverdueInvoices,
		0, -- SumOfPaidInvoices,
		0, -- TotalSumOfExpenses,
		0  -- TotalSumOfOrders
	FROM
		#order_items

	------------------------------------------------------------------------------

	UPDATE #months SET
		NextMonth = DATEADD(second, -1, DATEADD(month, 1, TheMonth))

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfOrders.
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
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 1
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate Turnover and TotalSumOfOrders.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		Turnover = ISNULL(d.Value, 0),
		TotalSumOfOrders = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 1
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate SumOfPaidInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		SumOfPaidInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 1
					AND i.status_or_cat = 'Paid'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate SumOfOverdueInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		SumOfOverdueInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 1
					AND i.status_or_cat = 'Overdue'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate SumOfOpenInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		SumOfOpenInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 1
					AND i.status_or_cat = 'Open'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate SumOfDraftInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		SumOfDraftInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(i.value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 1
					AND i.status_or_cat = 'Draft'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth
		

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfExpenses.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfExpenses = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 0
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfExpenses.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfExpenses = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 0
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate SumOfAdminExpensesCategory.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		SumOfAdminExpensesCategory = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 0
					AND i.status_or_cat = 'admin_expenses_categories'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate SumOfCostOfSalesExpensesCategory.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		SumOfCostOfSalesExpensesCategory = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 0
					AND i.status_or_cat = 'cost_of_sales_categories'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate SumOfGeneralExpensesCategory.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		SumOfGeneralExpensesCategory = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(ISNULL(value, 0))
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.dated_on BETWEEN im.TheMonth AND im.NextMonth
					AND i.IsInvoice = 0
					AND i.status_or_cat = 'general_categories'
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

	UPDATE FreeAgentAggregation SET
		IsActive = 0
	FROM
		FreeAgentAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #months m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO FreeAgentAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		NumOfExpenses,
		NumOfOrders,
		SumOfAdminExpensesCategory,
		SumOfCostOfSalesExpensesCategory,
		SumOfDraftInvoices,
		SumOfGeneralExpensesCategory,
		SumOfOpenInvoices,
		SumOfOverdueInvoices,
		SumOfPaidInvoices,
		TotalSumOfExpenses,
		TotalSumOfOrders
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		NumOfExpenses,
		NumOfOrders,
		SumOfAdminExpensesCategory,
		SumOfCostOfSalesExpensesCategory,
		SumOfDraftInvoices,
		SumOfGeneralExpensesCategory,
		SumOfOpenInvoices,
		SumOfOverdueInvoices,
		SumOfPaidInvoices,
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
	DROP TABLE #fa_id
END
GO
