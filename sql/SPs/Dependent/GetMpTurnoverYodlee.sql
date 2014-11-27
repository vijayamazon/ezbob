IF OBJECT_ID('GetMpTurnoverYodlee') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverYodlee AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverYodlee
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

	DECLARE @IsParsedBank BIT = 0

	------------------------------------------------------------------------------

	EXECUTE AdjustTurnoveDatesAndMonthCount	@MpID, @MonthCount OUTPUT, @DateTo OUTPUT, @DateFrom OUTPUT

	------------------------------------------------------------------------------

	EXECUTE GetYodleeRevenues
		@MpID, @DateFrom, @DateTo,
		@Turnover OUTPUT, @IsParsedBank OUTPUT, @TurnoverFrom OUTPUT, @TurnoverTo OUTPUT, @TurnoverDayCount OUTPUT

	------------------------------------------------------------------------------

	-- Ugly patch: currently Yodlee CSV (@IsParsedBank = 1) contains up to 12 months of data
	-- while Yodlee direct (@IsParsedBank = 0) contains up to 3 month of data: i.e. when
	-- we request data for the last year from Yodlee direct we receive only 3 months of data.
	-- Hence this multiplication by 4 if number of days is about 3 months.
	
	IF @IsParsedBank = 0 AND @MonthCount = 12 AND DATEDIFF(day, @TurnoverFrom, @TurnoverTo) < 100
		SET @Turnover = @Turnover * 4.0

	------------------------------------------------------------------------------

	SELECT
		RowType          = 'Turnover',
		MpID             = @MpID,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'),
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
