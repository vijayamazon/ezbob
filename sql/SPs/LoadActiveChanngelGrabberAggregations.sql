SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActiveChanngelGrabberAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveChanngelGrabberAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActiveChanngelGrabberAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.ChannelGrabberAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.AverageSumOfExpensesDenominator,
		a.AverageSumOfExpensesNumerator,
		a.AverageSumOfOrdersDenominator,
		a.AverageSumOfOrdersNumerator,
		a.NumOfExpenses,
		a.NumOfOrders,
		a.TotalSumOfExpenses,
		a.TotalSumOfOrders
	FROM
		ChannelGrabberAggregation a
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
