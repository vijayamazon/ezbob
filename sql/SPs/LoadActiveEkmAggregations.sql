SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActiveEkmAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveEkmAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActiveEkmAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.EkmAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.AverageSumOfCancelledOrderDenominator,
		a.AverageSumOfCancelledOrderNumerator,
		a.AverageSumOfOrderDenominator,
		a.AverageSumOfOrderNumerator,
		a.AverageSumOfOtherOrderDenominator,
		a.AverageSumOfOtherOrderNumerator,
		a.CancellationRateDenominator,
		a.CancellationRateNumerator,
		a.NumOfCancelledOrders,
		a.NumOfOrders,
		a.NumOfOtherOrders,
		a.TotalSumOfCancelledOrders,
		a.TotalSumOfOrders,
		a.TotalSumOfOtherOrders
	FROM
		EkmAggregation a
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
