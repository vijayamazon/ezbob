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
	DECLARE @DayCount DECIMAL(18, 2)

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

	IF @IsParsedBank = 0 AND @MonthCount = 12
	BEGIN
		SET @DayCount = CONVERT(DECIMAL(18, 2), DATEDIFF(day, @TurnoverFrom, @TurnoverTo))

		IF 60 <= @DayCount AND @DayCount <= 90
			SET @DayCount = 90.0

		IF @DayCount != 0
			SET @Turnover = @Turnover / @DayCount * 365.0
	END

	------------------------------------------------------------------------------

	SELECT
		RowType          = 'Turnover',
		MpID             = m.Id,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'),
		TurnoverType     = 'Total',
		Turnover         = @Turnover,
		MonthCount       = @MonthCount,
		DayCount         = @TurnoverDayCount,
		DateFrom         = @TurnoverFrom,
		DateTo           = @TurnoverTo,
		IsPaymentAccount = CONVERT(BIT, 1),
		LastUpdateTime   = m.UpdatingEnd
	FROM
		MP_CustomerMarketPlace m
	WHERE
		m.Id = @MpID

	------------------------------------------------------------------------------
END
GO
