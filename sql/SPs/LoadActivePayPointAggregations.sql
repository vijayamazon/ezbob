SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActivePayPointAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActivePayPointAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActivePayPointAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.PayPointAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.CancellationRateDenominator,
		a.CancellationRateNumerator,
		a.CancellationValue,
		a.NumOfFailures,
		a.NumOfOrders,
		a.OrdersAverageDenominator,
		a.OrdersAverageNumerator,
		a.SumOfAuthorisedOrders
	FROM
		PayPointAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON h.Id = a.CustomerMarketPlaceUpdatingHistoryID
			AND h.CustomerMarketPlaceId = @MpID
	WHERE
		a.IsActive = 1
		AND
		a.TheMonth < @RelevantDate
	ORDER BY
		a.TheMonth DESC
END
GO
