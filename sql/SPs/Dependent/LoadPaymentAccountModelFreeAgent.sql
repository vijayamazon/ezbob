IF OBJECT_ID('LoadPaymentAccountModelFreeAgent') IS NULL
	EXECUTE('CREATE PROCEDURE LoadPaymentAccountModelFreeAgent AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadPaymentAccountModelFreeAgent
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
			a.TotalSumOfOrders
		FROM
			FreeAgentAggregation a
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
			a.NumOfExpenses,
			a.NumOfOrders,
			a.TotalSumOfExpenses,
			a.TotalSumOfOrders,
			Pos = ROW_NUMBER() OVER (PARTITION BY a.TheMonth ORDER BY h.UpdatingEnd DESC, h.Id DESC)
		FROM
			FreeAgentAggregation a
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
		TotalNetInPayments  = SUM(t.TotalSumOfOrders),
		TransactionsNumber  = SUM(t.NumOfExpenses) + SUM(t.NumOfOrders),
		TotalNetOutPayments = SUM(t.TotalSumOfExpenses)
	FROM
		totals t
	WHERE
		t.Pos = 1
END
GO