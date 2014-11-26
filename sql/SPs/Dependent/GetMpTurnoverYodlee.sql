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
