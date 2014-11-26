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


	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(i.TotalCost),
		@TurnoverDayCount = COUNT(DISTINCT CONVERT(DATE, i.OrderDate)),
		@TurnoverFrom = MIN(i.OrderDate),
		@TurnoverTo = MAX(i.OrderDate)

	------------------------------------------------------------------------------
	
	SELECT
		RowType          = 'Turnover',
		MpID             = @MpID,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'),
		TurnoverType     = 'Total',
		Turnover         = @Turnover,
		Annualized       = (CASE @TurnoverDayCount WHEN 0 THEN 0 ELSE @Turnover / @MonthCount * 12.0 END),
		MonthCount       = @MonthCount,
		DayCount         = @TurnoverDayCount,
		DateFrom         = @TurnoverFrom,
		DateTo           = @TurnoverTo

	------------------------------------------------------------------------------
END
GO
