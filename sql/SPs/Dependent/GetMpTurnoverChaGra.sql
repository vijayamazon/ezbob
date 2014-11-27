IF OBJECT_ID('GetMpTurnoverChaGra') IS NULL
	EXECUTE('CREATE PROCEDURE GetMpTurnoverChaGra AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMpTurnoverChaGra
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
		i.NativeOrderId,
		MAX(i.Id) AS Id
	INTO
		#chagra
	FROM
		MP_ChannelGrabberOrder o
		INNER JOIN MP_ChannelGrabberOrderItem i ON o.Id = i.OrderId
	WHERE
		o.CustomerMarketPlaceId = @MpID
		AND
		@DateFrom <= i.PaymentDate AND i.PaymentDate <= @DateTo
	GROUP BY
		i.NativeOrderId

	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(ISNULL(i.TotalCost, 0)),
		@TurnoverDayCount = ISNULL(COUNT(DISTINCT CONVERT(DATE, i.PaymentDate)), 0),
		@TurnoverFrom = MIN(i.PaymentDate),
		@TurnoverTo = MAX(i.PaymentDate)
	FROM
		MP_ChannelGrabberOrderItem i
		INNER JOIN #chagra ON i.Id = #chagra.Id

	------------------------------------------------------------------------------
	
	SELECT
		RowType          = 'Turnover',
		MpID             = @MpID,
		MpTypeInternalID = mt.InternalId,
		TurnoverType     = 'Total',
		Turnover         = @Turnover,
		MonthCount       = @MonthCount,
		DayCount         = @TurnoverDayCount,
		DateFrom         = @TurnoverFrom,
		DateTo           = @TurnoverTo,
		IsPaymentAccount = mt.IsPaymentAccount
	FROM
		MP_CustomerMarketPlace cmp
		INNER JOIN MP_MarketplaceType mt ON cmp.MarketPlaceId = mt.Id
	WHERE
		cmp.Id = @MpID

	------------------------------------------------------------------------------

	DROP TABLE #chagra
END
GO
