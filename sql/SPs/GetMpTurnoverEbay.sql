IF OBJECT_ID('GetMpTurnoverEbay') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverEbay AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------
--
-- Date ranges are inclusive.
--
-- Limitations:
-- 1. Terapeak currently brings data aggregated per calendar month.
-- 2. If today is the 20th of the month turnover for last 3 months is
--    calculated as a turnover of this month (existing part) + turnover of
--    two previous months.
--
-- Thus @MonthCount logic:
--
-- 0: calculate turnover of current month. I.e. if today is the 1st then 
--    calculate turnover of the previous month, while if today is the 20th
--    calculate turnover since the 1st till today.
--
-- 1: calculate turnover for one month back. I.e. if today is the 1st then 
--    calculate turnover of the previous month, while if today is the 20th
--    calculate turnover since the 1st of the previous month till today.
--
-- 3: (or other n > 1) calculate turnover for 3 months back. I.e. if today
--    is the 1st then calculate turnover of the 3 previous months, while if
--    today is the 20th calculate turnover since the 1st of two months ago
--    till today.
--
-- Examples:
--
-- 0 months, April 1st: calculate March's turnover.
-- 0 months, April 10th: calculate turnover since April 1st till April 10th.
--
-- 1 month, April 1st: calculate March's turnover.
-- 1 month, April 10th: calculate turnover since March 1st till April 10th.
--
-- 3 months, April 1st: calculate turnover since January 1st till March 31st.
-- 3 months, April 10th: calculate turnover since February 1st till April 10th.
--
-------------------------------------------------------------------------------

ALTER PROCEDURE GetMpTurnoverEbay
@MpID INT,
@MonthCount INT,
@DateTo DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MaxOne INT

	DECLARE @DateFrom DATETIME

	DECLARE @TerapeakSum DECIMAL(18, 2)
	DECLARE @TerapeakDayCount INT
	DECLARE @TerapeakFrom DATETIME
	DECLARE @TerapeakTo DATETIME

	DECLARE @EbaySum DECIMAL(18, 2)
	DECLARE @EbayDayCount INT
	DECLARE @EbayFrom DATETIME
	DECLARE @EbayTo DATETIME

	------------------------------------------------------------------------------

	IF @MonthCount IS NULL
		SET @MonthCount = 0

	SET @MonthCount = ABS(@MonthCount)

	------------------------------------------------------------------------------

	IF @DateTo IS NULL
	BEGIN
		SELECT
			@DateTo = MAX(i.EndDate)
		FROM
			MP_TeraPeakOrderItem i
			INNER JOIN MP_TeraPeakOrder o ON i.TeraPeakOrderId = o.Id
		WHERE
			o.CustomerMarketPlaceId = @MpID
			AND
			i.RangeMarker IN (0, 1)
	END

	IF @DateTo IS NULL
		SET @DateTo = GETUTCDATE()

	-- Convert @DateTo's time part to 23:59:59 without changing a date part
	SET @DateTo = DATEADD(second, -1, DATEADD(day, 1, CONVERT(DATETIME, CONVERT(DATE, @DateTo))))

	------------------------------------------------------------------------------

	IF @MonthCount = 0
	BEGIN
		IF DATEPART(day, @DateTo) = 1
			SET @DateTo = DATEADD(day, -1, @DateTo)

		SET @DateFrom = DATEADD(month, DATEDIFF(month, 0, @DateTo), 0)
	END
	ELSE IF @MonthCount = 1
	BEGIN
		IF DATEPART(day, @DateTo) = 1
		BEGIN
			SET @DateTo = DATEADD(day, -1, @DateTo)
			SET @DateFrom = DATEADD(month, DATEDIFF(month, 0, @DateTo), 0)
		END
		ELSE BEGIN
			SET @DateFrom = DATEADD(month, -1, @DateTo)
			SET @DateFrom = DATEADD(month, DATEDIFF(month, 0, @DateFrom), 0)
		END
	END
	ELSE BEGIN
		IF DATEPART(day, @DateTo) = 1
		BEGIN
			SET @DateFrom = DATEADD(month, -@MonthCount, @DateTo)
			SET @DateTo = DATEADD(day, -1, @DateTo)
		END
		ELSE BEGIN
			SET @DateFrom = DATEADD(month, -(@MonthCount - 1), @DateTo)
			SET @DateFrom = DATEADD(month, DATEDIFF(month, 0, @DateFrom), 0)
		END
	END

	-- Convert @DateFrom's time part to 0:00:00 without changing a date part
	SET @DateFrom = CONVERT(DATETIME, CONVERT(DATE, @DateFrom))

	------------------------------------------------------------------------------

	SELECT
		i.StartDate,
		i.EndDate,
		MAX(i.Id) AS Id
	INTO
		#latest_data
	FROM
		MP_TeraPeakOrderItem i
		INNER JOIN MP_TeraPeakOrder o ON i.TeraPeakOrderId = o.Id
	WHERE
		o.CustomerMarketPlaceId = @MpID
		AND
		i.RangeMarker = 0
		AND (
			(@DateFrom <= i.StartDate AND i.StartDate <= @DateTo)
			OR
			(@DateFrom <= i.EndDate AND i.EndDate <= @DateTo)
		)
	GROUP BY
		i.StartDate,
		i.EndDate

	------------------------------------------------------------------------------

	SELECT
		@MaxOne = MAX(i.Id)
	FROM
		MP_TeraPeakOrderItem i
		INNER JOIN MP_TeraPeakOrder o ON i.TeraPeakOrderId = o.Id
	WHERE
		o.CustomerMarketPlaceId = @MpID
		AND
		i.RangeMarker = 1
		AND (
			(@DateFrom <= i.StartDate AND i.StartDate <= @DateTo)
			OR
			(@DateFrom <= i.EndDate AND i.EndDate <= @DateTo)
		)

	------------------------------------------------------------------------------

	INSERT INTO #latest_data (StartDate, EndDate, Id)
	SELECT DISTINCT
		i.StartDate,
		o.Created,
		i.Id
	FROM
		MP_TeraPeakOrderItem i
		INNER JOIN MP_TeraPeakOrder o ON i.TeraPeakOrderId = o.Id
		LEFT JOIN #latest_data ld ON
			(ld.StartDate <= i.StartDate AND i.StartDate <= ld.EndDate)
			OR
			(ld.StartDate <= o.Created AND o.Created <= ld.EndDate)
	WHERE
		ld.Id IS NULL
		AND
		i.Id = @MaxOne

	------------------------------------------------------------------------------

	SELECT
		@TerapeakSum = SUM(i.Revenue),
		@TerapeakDayCount = SUM(DATEDIFF(day, ld.StartDate, ld.EndDate) + 1),
		@TerapeakFrom = MIN(ld.StartDate),
		@TerapeakTo = MAX(ld.EndDate)
	FROM
		MP_TeraPeakOrderItem i
		INNER JOIN #latest_data ld ON i.Id = ld.Id

	------------------------------------------------------------------------------

	SELECT DISTINCT
		i.Id
	INTO
		#ebay
	FROM
		MP_EbayOrderItem i
		INNER JOIN MP_EbayOrder o
			ON i.OrderId = o.CustomerMarketPlaceId
			AND o.CustomerMarketPlaceId = @MpID
			AND i.OrderStatus IN ('Completed', 'Authenticated', 'Shipped')
		LEFT JOIN #latest_data ld
			ON ld.StartDate <= i.CreatedTime AND i.CreatedTime <= ld.EndDate
	WHERE
		@DateFrom <= i.CreatedTime AND i.CreatedTime <= @DateTo
		AND
		ld.Id IS NULL

	------------------------------------------------------------------------------

	SELECT
		@EbaySum = SUM(i.TotalAmount),
		@EbayDayCount = COUNT(DISTINCT CONVERT(DATE, i.CreatedTime)),
		@EbayFrom = MIN(i.CreatedTime),
		@EbayTo = MAX(i.CreatedTime)
	FROM
		MP_EbayOrderItem i
		INNER JOIN #ebay e ON i.Id = e.Id

	------------------------------------------------------------------------------
	
	CREATE TABLE #out (
		Pos INT,
		Name NVARCHAR(32),
		Turnover DECIMAL(18, 2),
		DayCount DECIMAL(18, 2),
		DateFrom DATETIME,
		DateTo DATETIME
	)

	------------------------------------------------------------------------------

	INSERT INTO #out (Pos, Name, Turnover, DayCount, DateFrom, DateTo) VALUES (
		2,
		'Terapeak',
		ISNULL(@TerapeakSum, 0),
		ISNULL(@TerapeakDayCount, 0),
		@TerapeakFrom,
		@TerapeakTo
	)

	------------------------------------------------------------------------------

	INSERT INTO #out (Pos, Name, Turnover, DayCount, DateFrom, DateTo) VALUES (
		3,
		'Ebay',
		ISNULL(@EbaySum, 0),
		ISNULL(@EbayDayCount, 0),
		@EbayFrom,
		@EbayTo
	)

	------------------------------------------------------------------------------

	INSERT INTO #out (Pos, Name, Turnover, DayCount, DateFrom, DateTo)
	SELECT
		1,
		'Total',
		SUM(Turnover),
		SUM(DayCount),
		MIN(DateFrom),
		MAX(DateTo)
	FROM
		#out

	------------------------------------------------------------------------------

	SELECT
		RowType          = 'Turnover',
		MpID             = @MpID,
		MpTypeInternalID = 'A7120CB7-4C93-459B-9901-0E95E7281B59',
		TurnoverType     = o.Name,
		Turnover         = o.Turnover,
		Annualized       = (CASE o.DayCount WHEN 0 THEN 0 ELSE o.Turnover / o.DayCount * 365.0 END),
		MonthCount       = @MonthCount,
		DayCount         = o.DayCount,
		DateFrom         = o.DateFrom,
		DateTo           = o.DateTo
	FROM
		#out o
	ORDER BY
		o.Pos

	------------------------------------------------------------------------------

	DROP TABLE #out
	DROP TABLE #ebay
	DROP TABLE #latest_data
END
GO
