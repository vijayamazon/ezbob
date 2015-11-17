SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActiveHmrcAggregations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveHmrcAggregations AS SELECT 1')
GO

ALTER PROCEDURE LoadActiveHmrcAggregations
@MpID INT,
@RelevantDate DATETIME
AS
BEGIN
	SELECT
		a.HmrcAggregationID,
		a.TheMonth,
		a.IsActive,
		a.Turnover,
		a.ValueAdded,
		a.FreeCashFlow
	FROM
		HmrcAggregation a
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
