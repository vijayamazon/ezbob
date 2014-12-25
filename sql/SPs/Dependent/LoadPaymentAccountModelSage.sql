IF OBJECT_ID('LoadPaymentAccountModelSage') IS NULL
	EXECUTE('CREATE PROCEDURE LoadPaymentAccountModelSage AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadPaymentAccountModelSage
@MpID INT,
@Now DATETIME,
@ShowCurrent BIT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @CurrentMonthEnd DATETIME
	DECLARE @CurrentMonthStart DATETIME
	DECLARE @YearAgo DATETIME

	------------------------------------------------------------------------------

	EXECUTE LoadPaymentAccountModelGetDates
		@MpID, @Now,
		@ShowCurrent OUTPUT,
		@CurrentMonthEnd OUTPUT,
		@CurrentMonthStart OUTPUT,
		@YearAgo OUTPUT

	------------------------------------------------------------------------------

	DECLARE @MonthInPayments NUMERIC(18, 2) = (
		SELECT TOP 1
			a.TotalSumOfOrders + a.TotalSumOfIncomes
		FROM
			SageAggregation a
			INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
				ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
				AND h.CustomerMarketPlaceId = @MpID
		WHERE
			a.TheMonth = @CurrentMonthStart
			AND (
				(@ShowCurrent = 1 AND a.IsActive = 1)
				OR
				(@ShowCurrent = 0 AND h.UpdatingEnd < @Now)
			)
		ORDER BY
			h.UpdatingEnd DESC,
			h.Id DESC
	)
		
	------------------------------------------------------------------------------

	;WITH totals AS (
		SELECT
			a.TheMonth,
			a.NumOfOrders,
			a.NumOfPurchaseInvoices,
			a.NumOfIncomes,
			a.NumOfExpenditures,
			a.TotalSumOfOrders,
			a.TotalSumOfPurchaseInvoices,
			a.TotalSumOfIncomes,
			a.TotalSumOfExpenditures,
			Pos = ROW_NUMBER() OVER (PARTITION BY a.TheMonth ORDER BY h.UpdatingEnd DESC, h.Id DESC)
		FROM
			SageAggregation a
			INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
				ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
				AND h.CustomerMarketPlaceId = @MpID
		WHERE
			a.TheMonth BETWEEN @YearAgo AND @CurrentMonthEnd
			AND (
				(@ShowCurrent = 1 AND a.IsActive = 1)
				OR
				(@ShowCurrent = 0 AND h.UpdatingEnd < @Now)
			)
	)
	SELECT
		MonthInPayments     = @MonthInPayments,
		TotalNetInPayments  = SUM(t.TotalSumOfOrders) + SUM(t.TotalSumOfIncomes),
		TransactionsNumber  = SUM(t.NumOfOrders) + SUM(t.NumOfPurchaseInvoices) + SUM(t.NumOfIncomes) + SUM(t.NumOfExpenditures),
		TotalNetOutPayments = SUM(t.TotalSumOfPurchaseInvoices) + SUM(t.TotalSumOfExpenditures)
	FROM
		totals t
	WHERE
		t.Pos = 1
END
GO
