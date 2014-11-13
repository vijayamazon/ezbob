IF OBJECT_ID('GetMpTurnoverPayPal') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverPayPal AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverPayPal
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

	SELECT
		o.Id
	INTO
		#trn
	FROM
		MP_PayPalTransaction o
	WHERE
		o.CustomerMarketPlaceId = @MpID

	------------------------------------------------------------------------------

	IF @DateTo IS NULL
	BEGIN
		SELECT
			@DateTo = MAX(i.Created)
		FROM
			MP_PayPalTransactionItem2 i
			INNER JOIN #trn o ON i.TransactionId = o.Id
	END

	------------------------------------------------------------------------------

	EXECUTE AdjustTurnoveDatesAndMonthCount @MonthCount OUTPUT, @DateTo OUTPUT, @DateFrom OUTPUT

	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(i.NetAmount),
		@TurnoverDayCount = COUNT(DISTINCT CONVERT(DATE, i.Created)),
		@TurnoverFrom = MIN(i.Created),
		@TurnoverTo = MAX(i.Created)
	FROM
		#trn o
		INNER JOIN MP_PayPalTransactionItem2 i
			ON o.Id = i.TransactionId
			AND i.Status = 'Completed'
			AND i.Type = 'Payment'
			AND i.NetAmount > 0
	WHERE
		@DateFrom <= i.Created AND i.Created <= @DateTo

	------------------------------------------------------------------------------
	
	SELECT
		RowType          = 'Turnover',
		MpID             = @MpID,
		MpTypeInternalID = '3FA5E327-FCFD-483B-BA5A-DC1815747A28',
		TurnoverType     = 'Total',
		Turnover         = @Turnover,
		Annualized       = (CASE @TurnoverDayCount WHEN 0 THEN 0 ELSE @Turnover / @TurnoverDayCount * 365.0 END),
		MonthCount       = @MonthCount,
		DayCount         = @TurnoverDayCount,
		DateFrom         = @TurnoverFrom,
		DateTo           = @TurnoverTo

	------------------------------------------------------------------------------

	DROP TABLE #trn
END
GO
