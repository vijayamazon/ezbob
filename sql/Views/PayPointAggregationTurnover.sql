SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('PayPointAggregationTurnover') IS NOT NULL
	DROP VIEW PayPointAggregationTurnover
GO

CREATE VIEW PayPointAggregationTurnover AS
	SELECT
		AggID = a.PayPointAggregationID,
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		PayPointAggregation a
		INNER JOIN MP_CustomerMarketplaceUpdatingHistory h
			ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
GO

