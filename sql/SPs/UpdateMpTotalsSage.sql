IF OBJECT_ID('UpdateMpTotalsSage') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsSage AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsSage
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @MpID INT

	EXECUTE GetMarketplaceFromHistoryID 'Sage', @HistoryID, @MpID OUTPUT

	IF @MpID IS NULL
		RETURN

	------------------------------------------------------------------------------

	DECLARE @SalesInvoice    INT = 1
	DECLARE @PurchaseInvoice INT = 2
	DECLARE @Income          INT = 3
	DECLARE @Expenditure     INT = 4

	------------------------------------------------------------------------------
	--
	-- Select relevant months.
	--
	------------------------------------------------------------------------------

	CREATE TABLE #month_list (
		TheMonth DATETIME NOT NULL,
		NextMonth DATETIME NULL
	)

	------------------------------------------------------------------------------

	INSERT INTO #month_list (TheMonth, NextMonth)
	SELECT DISTINCT
		dbo.udfMonthStart(i.[date]),
		NULL
	FROM
		MP_SageSalesInvoice i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID
	WHERE
		i.[date] IS NOT NULL
	--
	UNION
	--
	SELECT DISTINCT
		dbo.udfMonthStart(i.[date]),
		NULL
	FROM
		MP_SagePurchaseInvoice i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID
	WHERE
		i.[date] IS NOT NULL
	--
	UNION
	--
	SELECT
		dbo.udfMonthStart(i.[date]),
		NULL
	FROM
		MP_SageIncome i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID
	WHERE
		i.[date] IS NOT NULL
	--
	UNION
	--
	SELECT
		dbo.udfMonthStart(i.[date]),
		NULL
	FROM
		MP_SageExpenditure i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceUpdatingHistoryRecordId = @HistoryID
	WHERE
		i.[date] IS NOT NULL

	------------------------------------------------------------------------------

	UPDATE #month_list SET
		NextMonth = dbo.udfMonthEnd(TheMonth)

	------------------------------------------------------------------------------
	--
	-- Select relevant transaction id.
	--
	------------------------------------------------------------------------------

	CREATE TABLE #sage_id (
		TypeID INT NOT NULL,
		ItemID INT NOT NULL,
		SageID INT NOT NULL
	)

	------------------------------------------------------------------------------

	INSERT INTO #sage_id (TypeID, ItemID, SageID)
	SELECT
		@SalesInvoice,
		MAX(i.Id),
		i.SageId
	FROM
		MP_SageSalesInvoice i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceId = @MpID
		INNER JOIN #month_list ml
			ON i.[date] BETWEEN ml.TheMonth AND ml.NextMonth
	GROUP BY
		i.SageId

	-----------------------------------------------------------------------------

	INSERT INTO #sage_id (TypeID, ItemID, SageID)
	SELECT
		@PurchaseInvoice,
		MAX(i.Id),
		i.SageId
	FROM
		MP_SagePurchaseInvoice i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceId = @MpID
		INNER JOIN #month_list ml
			ON i.[date] BETWEEN ml.TheMonth AND ml.NextMonth
	GROUP BY
		i.SageId

	-----------------------------------------------------------------------------

	INSERT INTO #sage_id (TypeID, ItemID, SageID)
	SELECT
		@Income,
		MAX(i.Id),
		i.SageId
	FROM
		MP_SageIncome i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceId = @MpID
		INNER JOIN #month_list ml
			ON i.[date] BETWEEN ml.TheMonth AND ml.NextMonth
	GROUP BY
		i.SageId

	-----------------------------------------------------------------------------

	INSERT INTO #sage_id (TypeID, ItemID, SageID)
	SELECT
		@Expenditure,
		MAX(i.Id),
		i.SageId
	FROM
		MP_SageExpenditure i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceId = @MpID
		INNER JOIN #month_list ml
			ON i.[date] BETWEEN ml.TheMonth AND ml.NextMonth
	GROUP BY
		i.SageId

	------------------------------------------------------------------------------
	--
	-- Select relevant transaction id.
	--
	------------------------------------------------------------------------------

	CREATE TABLE #order_items (
		TypeID INT NOT NULL,
		ItemID INT NOT NULL,
		Value NUMERIC(18, 2) NOT NULL,
		ItemDate DATETIME NOT NULL,
		StatusID INT NULL
	)

	------------------------------------------------------------------------------

	INSERT INTO #order_items(TypeID, ItemID, Value, ItemDate, StatusID)
	SELECT
		id.TypeID,
		id.ItemID,
		i.total_net_amount,
		i.[date],
		i.StatusId
	FROM
		MP_SageSalesInvoice i
		INNER JOIN #sage_id id
			ON i.Id = id.ItemID
			AND id.TypeID = @SalesInvoice

	------------------------------------------------------------------------------

	INSERT INTO #order_items(TypeID, ItemID, Value, ItemDate, StatusID)
	SELECT
		id.TypeID,
		id.ItemID,
		i.total_net_amount,
		i.[date],
		i.StatusId
	FROM
		MP_SagePurchaseInvoice i
		INNER JOIN #sage_id id
			ON i.Id = id.ItemID
			AND id.TypeID = @PurchaseInvoice

	------------------------------------------------------------------------------

	INSERT INTO #order_items(TypeID, ItemID, Value, ItemDate, StatusID)
	SELECT
		id.TypeID,
		id.ItemID,
		i.amount,
		i.[date],
		NULL
	FROM
		MP_SageIncome i
		INNER JOIN #sage_id id
			ON i.Id = id.ItemID
			AND id.TypeID = @Income

	------------------------------------------------------------------------------

	INSERT INTO #order_items(TypeID, ItemID, Value, ItemDate, StatusID)
	SELECT
		id.TypeID,
		id.ItemID,
		i.amount,
		i.[date],
		NULL
	FROM
		MP_SageExpenditure i
		INNER JOIN #sage_id id
			ON i.Id = id.ItemID
			AND id.TypeID = @Expenditure

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
		NumOfExpenditures,
		NumOfIncomes,
		NumOfOrders,
		NumOfPurchaseInvoices,
		TotalSumOfExpenditures,
		TotalSumOfIncomes,
		TotalSumOfOrders,
		TotalSumOfPaidPurchaseInvoices,
		TotalSumOfPaidSalesInvoices,
		TotalSumOfPartiallyPaidPurchaseInvoices,
		TotalSumOfPartiallyPaidSalesInvoices,
		TotalSumOfPurchaseInvoices,
		TotalSumOfUnpaidPurchaseInvoices,
		TotalSumOfUnpaidSalesInvoices
	INTO
		#months
	FROM
		SageAggregation
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
		NumOfExpenditures,
		NumOfIncomes,
		NumOfOrders,
		NumOfPurchaseInvoices,
		TotalSumOfExpenditures,
		TotalSumOfIncomes,
		TotalSumOfOrders,
		TotalSumOfPaidPurchaseInvoices,
		TotalSumOfPaidSalesInvoices,
		TotalSumOfPartiallyPaidPurchaseInvoices,
		TotalSumOfPartiallyPaidSalesInvoices,
		TotalSumOfPurchaseInvoices,
		TotalSumOfUnpaidPurchaseInvoices,
		TotalSumOfUnpaidSalesInvoices
	)
	SELECT DISTINCT
		dbo.udfMonthStart(ItemDate),
		'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value in the next query.
		@HistoryID,
		0, -- Turnover,
		0, -- NumOfExpenditures,
		0, -- NumOfIncomes,
		0, -- NumOfOrders,
		0, -- NumOfPurchaseInvoices,
		0, -- TotalSumOfExpenditures,
		0, -- TotalSumOfIncomes,
		0, -- TotalSumOfOrders,
		0, -- TotalSumOfPaidPurchaseInvoices,
		0, -- TotalSumOfPaidSalesInvoices,
		0, -- TotalSumOfPartiallyPaidPurchaseInvoices,
		0, -- TotalSumOfPartiallyPaidSalesInvoices,
		0, -- TotalSumOfPurchaseInvoices,
		0, -- TotalSumOfUnpaidPurchaseInvoices,
		0  -- TotalSumOfUnpaidSalesInvoices
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
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @SalesInvoice
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfOrders.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfOrders = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @SalesInvoice
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfPaidSalesInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfPaidSalesInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @SalesInvoice
				INNER JOIN MP_SagePaymentStatus s
					ON i.StatusID = s.SageId
					AND s.name = 'Paid'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfUnpaidSalesInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfUnpaidSalesInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @SalesInvoice
				INNER JOIN MP_SagePaymentStatus s
					ON i.StatusID = s.SageId
					AND s.name = 'Unpaid'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfPartiallyPaidSalesInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfPartiallyPaidSalesInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @SalesInvoice
				INNER JOIN MP_SagePaymentStatus s
					ON i.StatusID = s.SageId
					AND s.name = 'Part Paid'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfPurchaseInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfPurchaseInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @PurchaseInvoice
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfPurchaseInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfPurchaseInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @PurchaseInvoice
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfPaidPurchaseInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfPaidPurchaseInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @PurchaseInvoice
				INNER JOIN MP_SagePaymentStatus s
					ON i.StatusID = s.SageId
					AND s.name = 'Paid'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfUnpaidPurchaseInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfUnpaidPurchaseInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @PurchaseInvoice
				INNER JOIN MP_SagePaymentStatus s
					ON i.StatusID = s.SageId
					AND s.name = 'Unpaid'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfPartiallyPaidPurchaseInvoices.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfPartiallyPaidPurchaseInvoices = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @PurchaseInvoice
				INNER JOIN MP_SagePaymentStatus s
					ON i.StatusID = s.SageId
					AND s.name = 'Part Paid'
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfIncomes.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfIncomes = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @Income
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate Turnover and TotalSumOfIncomes.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		Turnover = ISNULL(d.Value, 0),
		TotalSumOfIncomes = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @Income
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate NumOfExpenditures.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		NumOfExpenditures = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = COUNT(*)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @Expenditure
			GROUP BY
				im.TheMonth
		) d ON m.TheMonth = d.TheMonth

	------------------------------------------------------------------------------
	--
	-- Calculate TotalSumOfExpenditures.
	--
	------------------------------------------------------------------------------

	UPDATE #months SET
		TotalSumOfExpenditures = ISNULL(d.Value, 0)
	FROM
		#months m
		INNER JOIN (
			SELECT
				im.TheMonth,
				Value = SUM(i.Value)
			FROM
				#months im
				INNER JOIN #order_items i
					ON i.ItemDate BETWEEN im.TheMonth AND im.NextMonth
					AND i.TypeID = @Expenditure
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

	UPDATE SageAggregation SET
		IsActive = 0
	FROM
		SageAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #months m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO SageAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		NumOfExpenditures,
		NumOfIncomes,
		NumOfOrders,
		NumOfPurchaseInvoices,
		TotalSumOfExpenditures,
		TotalSumOfIncomes,
		TotalSumOfOrders,
		TotalSumOfPaidPurchaseInvoices,
		TotalSumOfPaidSalesInvoices,
		TotalSumOfPartiallyPaidPurchaseInvoices,
		TotalSumOfPartiallyPaidSalesInvoices,
		TotalSumOfPurchaseInvoices,
		TotalSumOfUnpaidPurchaseInvoices,
		TotalSumOfUnpaidSalesInvoices
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		NumOfExpenditures,
		NumOfIncomes,
		NumOfOrders,
		NumOfPurchaseInvoices,
		TotalSumOfExpenditures,
		TotalSumOfIncomes,
		TotalSumOfOrders,
		TotalSumOfPaidPurchaseInvoices,
		TotalSumOfPaidSalesInvoices,
		TotalSumOfPartiallyPaidPurchaseInvoices,
		TotalSumOfPartiallyPaidSalesInvoices,
		TotalSumOfPurchaseInvoices,
		TotalSumOfUnpaidPurchaseInvoices,
		TotalSumOfUnpaidSalesInvoices
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
	DROP TABLE #sage_id
	DROP TABLE #month_list
END
GO
