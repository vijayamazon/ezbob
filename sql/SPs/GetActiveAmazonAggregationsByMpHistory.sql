SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetActiveAmazonAggregationsByMpHistory') IS NULL
	EXECUTE('CREATE PROCEDURE GetActiveAmazonAggregationsByMpHistory AS SELECT 1')
GO

ALTER PROCEDURE GetActiveAmazonAggregationsByMpHistory
@HistoryRecordIDs IntList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.TheMonth,
		a.Turnover,
		a.CustomerMarketPlaceUpdatingHistoryID,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		AmazonAggregation a
		INNER JOIN @HistoryRecordIDs r ON a.CustomerMarketPlaceUpdatingHistoryID = r.Value
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
	WHERE
		a.IsActive = 1
END
GO
