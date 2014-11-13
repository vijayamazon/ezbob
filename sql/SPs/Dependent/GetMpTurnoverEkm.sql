IF OBJECT_ID('GetMpTurnoverEkm') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverEkm AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverEkm
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

	IF @DateTo IS NULL
	BEGIN
		SELECT
			@DateTo = MAX(i.OrderDate)
		FROM
			MP_EkmOrderItem i
			INNER JOIN MP_EkmOrder o ON i.OrderId = o.Id
		WHERE
			o.CustomerMarketPlaceId = @MpID
	END

	------------------------------------------------------------------------------

	EXECUTE AdjustTurnoveDatesAndMonthCount @MonthCount OUTPUT, @DateTo OUTPUT, @DateFrom OUTPUT

	------------------------------------------------------------------------------

	SELECT
		i.OrderNumber,
		MAX(i.Id) AS Id
	INTO
		#ekm
	FROM
		MP_EkmOrder o
		INNER JOIN MP_EkmOrderItem i ON o.Id = i.OrderId
	WHERE
		o.CustomerMarketPlaceId = @MpID
		AND
		@DateFrom <= i.OrderDate AND i.OrderDate <= @DateTo
	GROUP BY
		i.OrderNumber

	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(i.TotalCost),
		@TurnoverDayCount = COUNT(DISTINCT CONVERT(DATE, i.OrderDate)),
		@TurnoverFrom = MIN(i.OrderDate),
		@TurnoverTo = MAX(i.OrderDate)
	FROM
		MP_EkmOrder o
		INNER JOIN MP_EkmOrderItem i
			ON o.Id = i.OrderId
			AND LOWER(LTRIM(RTRIM(i.OrderStatus))) NOT IN ('refunded', 'failed', 'cancelled')
		INNER JOIN #ekm ON i.Id = #ekm.Id

	------------------------------------------------------------------------------
	
	SELECT
		RowType          = 'Turnover',
		MpID             = @MpID,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, '57ABA690-EDBA-4D95-89CF-13A34B40E2F1'),
		TurnoverType     = 'Total',
		Turnover         = @Turnover,
		Annualized       = (CASE @TurnoverDayCount WHEN 0 THEN 0 ELSE @Turnover / @TurnoverDayCount * 365.0 END),
		MonthCount       = @MonthCount,
		DayCount         = @TurnoverDayCount,
		DateFrom         = @TurnoverFrom,
		DateTo           = @TurnoverTo

	------------------------------------------------------------------------------

	DROP TABLE #ekm
END
GO
