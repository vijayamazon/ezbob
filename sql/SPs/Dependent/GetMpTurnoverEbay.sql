IF OBJECT_ID('GetMpTurnoverEbay') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverEbay AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverEbay
@MpID INT,
@MonthCount INT,
@DateTo DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MaxDate DATETIME
	DECLARE @MinDate DATETIME

	DECLARE @MaxOne INT
	DECLARE @TwoID INT

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

	DECLARE @eBay UNIQUEIDENTIFIER = CONVERT(UNIQUEIDENTIFIER, 'A7120CB7-4C93-459B-9901-0E95E7281B59')
	DECLARE @Turnover NVARCHAR(8) = 'Turnover'

	------------------------------------------------------------------------------

	EXECUTE AdjustTurnoveDatesAndMonthCount @MpID, @MonthCount OUTPUT, @DateTo OUTPUT, @DateFrom OUTPUT

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

	IF @MonthCount = 1
	BEGIN
		SELECT TOP 1
			@TwoID = i.Id
		FROM
			MP_TeraPeakOrderItem i
			INNER JOIN MP_TeraPeakOrder o
				ON i.TeraPeakOrderId = o.Id
				AND o.CustomerMarketPlaceId = @MpID
		WHERE
			i.RangeMarker = 2
			AND
			ABS(DATEDIFF(hour, i.EndDate, @DateTo)) <= 23
		ORDER BY
			i.Id DESC

		-------------------------------------------------------------------------

		IF @TwoID IS NOT NULL
		BEGIN
			INSERT INTO #out (Pos, Name, Turnover, DayCount, DateFrom, DateTo)
			SELECT
				1,
				'Total',
				ISNULL(i.Revenue, 0),
				ISNULL(DATEDIFF(day, i.StartDate, i.EndDate), 0),
				i.StartDate,
				i.EndDate
			FROM
				MP_TeraPeakOrderItem i
			WHERE
				i.Id = @TwoID

			--------------------------------------------------------------------

			SELECT
				RowType          = @Turnover,
				MpID             = @MpID,
				MpTypeInternalID = @eBay,
				TurnoverType     = o.Name,
				Turnover         = o.Turnover,
				Annualized       = (CASE o.DayCount WHEN 0 THEN 0 ELSE o.Turnover / @MonthCount * 12.0 END),
				MonthCount       = @MonthCount,
				DayCount         = o.DayCount,
				DateFrom         = o.DateFrom,
				DateTo           = o.DateTo
			FROM
				#out o
			ORDER BY
				o.Pos

			--------------------------------------------------------------------

			DROP TABLE #out

			--------------------------------------------------------------------

			RETURN
		END -- if found relevant RangeMarker = 2
	END -- if month count = 1

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
		@TerapeakSum = SUM(ISNULL(i.Revenue, 0)),
		@TerapeakDayCount = SUM(ISNULL(DATEDIFF(day, ld.StartDate, ld.EndDate), 0) + 1),
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
		@EbaySum = SUM(ISNULL(i.TotalAmount, 0)),
		@EbayDayCount = ISNULL(COUNT(DISTINCT CONVERT(DATE, i.CreatedTime)), 0),
		@EbayFrom = MIN(i.CreatedTime),
		@EbayTo = MAX(i.CreatedTime)
	FROM
		MP_EbayOrderItem i
		INNER JOIN #ebay e ON i.Id = e.Id

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

	SELECT
		@MinDate = MIN(DateFrom)
	FROM
		#out
	WHERE
		DateFrom IS NOT NULL

	------------------------------------------------------------------------------

	SELECT
		@MaxDate = MAX(DateTo)
	FROM
		#out
	WHERE
		DateTo IS NOT NULL

	------------------------------------------------------------------------------

	INSERT INTO #out (Pos, Name, Turnover, DayCount, DateFrom, DateTo)
	SELECT
		1,
		'Total',
		SUM(ISNULL(Turnover, 0)),
		SUM(ISNULL(DayCount, 0)),
		@MinDate,
		@MaxDate
	FROM
		#out

	------------------------------------------------------------------------------

	SELECT
		RowType          = @Turnover,
		MpID             = @MpID,
		MpTypeInternalID = @eBay,
		TurnoverType     = o.Name,
		Turnover         = o.Turnover,
		Annualized       = (CASE o.DayCount WHEN 0 THEN 0 ELSE o.Turnover / @MonthCount * 12.0 END),
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
