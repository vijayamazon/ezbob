IF OBJECT_ID('GetMpTurnoverAmazon') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverAmazon AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverAmazon
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
		@Turnover = SUM(ISNULL(i.OrderTotal, 0)),
		@TurnoverDayCount = ISNULL(COUNT(DISTINCT CONVERT(DATE, i.LastUpdateDate)), 0),
		@TurnoverFrom = MIN(i.LastUpdateDate),
		@TurnoverTo = MAX(i.LastUpdateDate)
	FROM
		MP_AmazonOrder o
		INNER JOIN MP_AmazonOrderItem i
			ON o.Id = i.AmazonOrderId
			AND i.OrderStatus = 'Shipped'
	WHERE
		o.CustomerMarketPlaceId = @MpID
		AND
		@DateFrom <= i.LastUpdateDate AND i.LastUpdateDate <= @DateTo

	------------------------------------------------------------------------------
	
	SELECT
		RowType          = 'Turnover',
		MpID             = m.Id,
		MpTypeInternalID = CONVERT(UNIQUEIDENTIFIER, 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'),
		TurnoverType     = 'Total',
		Turnover         = @Turnover,
		MonthCount       = @MonthCount,
		DayCount         = @TurnoverDayCount,
		DateFrom         = @TurnoverFrom,
		DateTo           = @TurnoverTo,
		IsPaymentAccount = CONVERT(BIT, 0),
		LastUpdateTime   = m.UpdatingEnd
	FROM
		MP_CustomerMarketPlace m
	WHERE
		m.Id = @MpID

	------------------------------------------------------------------------------
END
GO
