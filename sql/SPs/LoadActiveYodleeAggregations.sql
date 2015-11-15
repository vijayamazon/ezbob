SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActiveYodleeAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveYodleeAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActiveYodleeAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.YodleeAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.NumberOfTransactions,
		a.TotalExpense,
		a.TotalIncome,
		a.NetCashFlow
	FROM
		YodleeAggregation a
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
