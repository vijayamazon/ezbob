IF OBJECT_ID('GetMpTurnoverSage') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverSage AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverSage
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
		i.SageId,
		MAX(i.Id) AS Id
	INTO
		#sage
	FROM
		MP_SageIncome i
		INNER JOIN MP_SageRequest o
			ON i.RequestId = o.Id
			AND o.CustomerMarketPlaceId = @MpID
	WHERE
		@DateFrom <= i.[date] AND i.[date] <= @DateTo
	GROUP BY
		i.SageId

	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(ISNULL(i.amount, 0)),
		@TurnoverDayCount = COUNT(DISTINCT CONVERT(DATE, i.[date])),
		@TurnoverFrom = MIN(i.[date]),
		@TurnoverTo = MAX(i.[date])
	FROM
		MP_SageIncome i
		INNER JOIN #sage ON i.Id = #sage.Id

	------------------------------------------------------------------------------
	
	SELECT
		RowType          = 'Turnover',
		MpID             = m.Id,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, '4966BB57-0146-4E3D-AA24-F092D90B7923'),
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

	DROP TABLE #sage
END
GO
