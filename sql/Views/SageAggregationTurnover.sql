SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SageAggregationTurnover') IS NOT NULL
	DROP VIEW SageAggregationTurnover
GO

CREATE VIEW SageAggregationTurnover AS
	SELECT
		AggID = a.SageAggregationID,
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		h.CustomerMarketPlaceId,
		h.UpdatingEnd
	FROM
		SageAggregation a
		INNER JOIN MP_CustomerMarketplaceUpdatingHistory h
			ON a.CustomerMarketPlaceUpdatingHistoryID = h.Id
GO

