IF OBJECT_ID('UpdateMpTotalsEkm') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsEkm AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsEkm
@MpID INT,
@History INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	------------------------------------------------------------------------------

	SELECT
		i.OrderNumber,
		MAX(i.Id) AS Id
	INTO
		#ekm
	FROM
		MP_EkmOrder o
		INNER JOIN MP_EkmOrderItem i ON o.Id = i.OrderId
	WHERE
		o.CustomerMarketPlaceId = @MpID
		AND
		@DateFrom <= i.OrderDate AND i.OrderDate <= @DateTo
	GROUP BY
		i.OrderNumber

	------------------------------------------------------------------------------

	SELECT
		@Turnover = SUM(i.TotalCost),
		@TurnoverDayCount = COUNT(DISTINCT CONVERT(DATE, i.OrderDate)),
		@TurnoverFrom = MIN(i.OrderDate),
		@TurnoverTo = MAX(i.OrderDate)
	FROM
		MP_EkmOrder o
		INNER JOIN MP_EkmOrderItem i
			ON o.Id = i.OrderId
			AND LOWER(LTRIM(RTRIM(i.OrderStatus))) NOT IN ('refunded', 'failed', 'cancelled')
		INNER JOIN #ekm ON i.Id = #ekm.Id

	------------------------------------------------------------------------------
	
	------------------------------------------------------------------------------

	DROP TABLE #ekm
END
GO
