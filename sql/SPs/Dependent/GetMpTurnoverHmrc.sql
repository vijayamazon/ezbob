IF OBJECT_ID('GetMpTurnoverHmrc') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverHmrc AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverHmrc
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
		n.Id
	INTO
		#boxes
	FROM
		MP_VatReturnEntryNames n
	WHERE
		n.Name LIKE '%(Box 6)%'

	------------------------------------------------------------------------------

	SELECT
		o.DateFrom,
		o.DateTo,
		o.BusinessId,
		MAX(o.Id) AS Id
	INTO
		#periods
	FROM
		MP_VatReturnRecords o
	WHERE
		ISNULL(o.IsDeleted, 0) = 0
		AND o.CustomerMarketPlaceId = @MpID
	GROUP BY
		o.DateFrom,
		o.DateTo,
		o.BusinessId

	------------------------------------------------------------------------------

	SELECT
		DateFrom   = dbo.udfMinDate(o.DateFrom, o.DateTo),
		DateTo     = dbo.udfMaxDate(o.DateFrom, o.DateTo),
		BusinessID = o.BusinessId,
		Amount     = i.Amount,
		Ratio      = dbo.udfDateIntersectionRatio(o.DateFrom, dbo.udfJustBeforeMidnight(o.DateTo), @DateFrom, @DateTo)
	INTO
		#tuples
	FROM
		MP_VatReturnEntries i
		INNER JOIN #periods o ON i.RecordId = o.Id
		INNER JOIN #boxes b ON i.NameId = b.Id
	WHERE
		ISNULL(i.IsDeleted, 0) = 0

	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(i.Amount * i.Ratio),
		@TurnoverFrom = MIN(i.DateFrom),
		@TurnoverTo = MAX(i.DateTo)
	FROM
		#tuples i
	WHERE
		i.Ratio != 0

	SET @TurnoverDayCount = DATEDIFF(day, ISNULL(@TurnoverFrom, @DateFrom), ISNULL(@TurnoverTo, @DateTo))

	------------------------------------------------------------------------------
	
	SELECT
		RowType          = 'Turnover',
		MpID             = @MpID,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'),
		TurnoverType     = 'Total',
		Turnover         = ISNULL(@Turnover, 0),
		Annualized       = (CASE @TurnoverDayCount WHEN 0 THEN 0 ELSE ISNULL(@Turnover, 0) / @MonthCount * 12.0 END),
		MonthCount       = @MonthCount,
		DayCount         = ISNULL(@TurnoverDayCount, 0),
		DateFrom         = ISNULL(@TurnoverFrom, @DateFrom),
		DateTo           = ISNULL(@TurnoverTo, @DateTo)

	------------------------------------------------------------------------------

	DROP TABLE #tuples
	DROP TABLE #periods
	DROP TABLE #boxes
END
GO
