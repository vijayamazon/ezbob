IF OBJECT_ID('GetMpTurnoverFreeAgent') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverFreeAgent AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverFreeAgent
@MpID INT,
@MonthCount INT,
@DateTo DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @DateFrom DATETIME

	DECLARE @Turnover DECIMAL(18, 2)
	DECLARE @TurnoverDayCount INT
	DECLARE @TurnoverFrom DATETIME
	DECLARE @TurnoverTo DATETIME

	------------------------------------------------------------------------------

	EXECUTE AdjustTurnoveDatesAndMonthCount @MpID, @MonthCount OUTPUT, @DateTo OUTPUT, @DateFrom OUTPUT

	------------------------------------------------------------------------------

	SELECT
		i.url,
		MAX(i.Id) AS Id
	INTO
		#fa
	FROM
		MP_FreeAgentInvoice i
		INNER JOIN MP_FreeAgentRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceId = @MpID
	WHERE
		@DateFrom <= i.dated_on AND i.dated_on <= @DateTo
	GROUP BY
		i.url

	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(ISNULL(i.net_value, 0) * ISNULL(i.exchange_rate, 0)),
		@TurnoverDayCount = COUNT(DISTINCT CONVERT(DATE, i.dated_on)),
		@TurnoverFrom = MIN(i.dated_on),
		@TurnoverTo = MAX(i.dated_on)
	FROM
		MP_FreeAgentInvoice i
		INNER JOIN #fa ON i.Id = #fa.Id

	------------------------------------------------------------------------------

	DECLARE @Rate NUMERIC(18, 8) = ISNULL((
		SELECT TOP 1
			h.Price
		FROM
			MP_CurrencyRateHistory h
			INNER JOIN MP_Currency c ON h.CurrencyId = c.Id
		WHERE
			c.Name LIKE 'USD%'
			AND
			CONVERT(DATE, h.Updated) = CONVERT(DATE, @TurnoverTo)
		ORDER BY
			h.Updated DESC
	), 1)

	IF @Rate = 0
		SET @Rate = 1

	------------------------------------------------------------------------------

	SELECT
		RowType          = 'Turnover',
		MpID             = @MpID,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, '737691E8-5C77-48EF-B01B-7348E24094B6'),
		TurnoverType     = 'Total',
		Turnover         = @Turnover / @Rate,
		MonthCount       = @MonthCount,
		DayCount         = @TurnoverDayCount,
		DateFrom         = @TurnoverFrom,
		DateTo           = @TurnoverTo,
		IsPaymentAccount = CONVERT(BIT, 1)

	------------------------------------------------------------------------------

	DROP TABLE #fa
END
GO
