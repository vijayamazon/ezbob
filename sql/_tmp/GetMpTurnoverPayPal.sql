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

	EXECUTE AdjustTurnoveDatesAndMonthCount @MpID, @MonthCount OUTPUT, @DateTo OUTPUT, @DateFrom OUTPUT

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
		MpID             = m.Id,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, '3FA5E327-FCFD-483B-BA5A-DC1815747A28'),
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

	DROP TABLE #trn
END
GO
