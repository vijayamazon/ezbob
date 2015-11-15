SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActiveAmazonAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveAmazonAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActiveAmazonAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.AmazonAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.AverageItemsPerOrderDenominator,
		a.AverageItemsPerOrderNumerator,
		a.AverageSumOfOrderDenominator,
		a.AverageSumOfOrderNumerator,
		a.CancelledOrdersCount,
		a.NumOfOrders,
		a.OrdersCancellationRateDenominator,
		a.OrdersCancellationRateNumerator,
		a.TotalItemsOrdered,
		a.TotalSumOfOrders
	FROM 
		AmazonAggregation a
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